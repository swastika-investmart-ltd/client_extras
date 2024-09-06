using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
//using System.Web.Mvc;

namespace Entities
{
    public class CustomValidators
    {
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
        public abstract class ConditionalValidationAttribute : ValidationAttribute
        {
            protected readonly ValidationAttribute InnerAttribute;
            public string DependentProperty { get; set; }
            public object TargetValue { get; set; }
            protected abstract string ValidationName { get; }

            protected virtual IDictionary<string, object> GetExtraValidationParameters()
            {
                return new Dictionary<string, object>();
            }

            protected ConditionalValidationAttribute(ValidationAttribute innerAttribute, string dependentProperty, object targetValue)
            {
                this.InnerAttribute = innerAttribute;
                this.DependentProperty = dependentProperty;
                this.TargetValue = targetValue;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                // get a reference to the property this validation depends upon    
                var containerType = validationContext.ObjectInstance.GetType();
                var field = containerType.GetProperty(this.DependentProperty);
                if (field != null)
                {
                    // get the value of the dependent property    
                    var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);

                    // compare the value against the target value    
                    if ((dependentvalue == null && this.TargetValue == null) || (dependentvalue != null && dependentvalue.Equals(this.TargetValue)))
                    {
                        // match => means we should try validating this field    
                        if (!InnerAttribute.IsValid(value))
                        {
                            // validation failed - return an error    
                            return new ValidationResult(this.ErrorMessage, new[] { validationContext.MemberName });
                        }
                    }
                }
                return ValidationResult.Success;
            }


        }

        public class RequiredIfAttribute : ConditionalValidationAttribute
        {
            protected override string ValidationName
            {
                get { return "requiredif"; }
            }
            public RequiredIfAttribute(string dependentProperty, object targetValue)
                : base(new RequiredAttribute(), dependentProperty, targetValue)
            {
            }
            protected override IDictionary<string, object> GetExtraValidationParameters()
            {
                return new Dictionary<string, object>
               {
                   { "rule", "required" }
               };
            }
        }
        public class RangeIfAttribute : ConditionalValidationAttribute
        {
            private readonly long minimum;
            private readonly long maximum;
            protected override string ValidationName
            {
                get { return "rangeif"; }
            }
            public RangeIfAttribute(long minimum, long maximum, string dependentProperty, object targetValue)
                : base(new RangeAttribute(minimum, maximum), dependentProperty, targetValue)
            {
                this.minimum = minimum;
                this.maximum = maximum;
            }
            protected override IDictionary<string, object> GetExtraValidationParameters()
            {
                // Set the rule Range and the rule param [minumum,maximum]    
                return new Dictionary<string, object> {
                 {"rule", "range"},
                 { "ruleparam", string.Format("[{0},{1}]", this.minimum, this.maximum) }
                };
            }
        }

        public class DecimalRangeIfAttribute : ConditionalValidationAttribute
        {
            private readonly decimal minimum;
            private readonly decimal maximum;
            protected override string ValidationName
            {
                get { return "decimalrangeif"; }
            }
            public DecimalRangeIfAttribute(decimal minimum, decimal maximum, string dependentProperty, object targetValue)
                : base(new RangeAttribute((double)minimum, (double)maximum), dependentProperty, targetValue)
            {
                this.minimum = minimum;
                this.maximum = maximum;
            }
            protected override IDictionary<string, object> GetExtraValidationParameters()
            {
                // Set the rule Range and the rule param [minumum,maximum]    
                return new Dictionary<string, object> {
                 {"rule", "range"},
                 { "ruleparam", string.Format("[{0},{1}]", this.minimum, this.maximum) }
                };
            }
        }

        //Minumum, no max
        //[EnsureMinimumElements(min: 1, ErrorMessage = "Select at least one item")]
        //Min and Max
        //[EnsureMinimumElements(min: 1, max: 6, ErrorMessage = "You can only add 1 to 6 items to your basket")]
        //No min
        //[EnsureMinimumElements(max: 6, ErrorMessage = "You can add upto 6 items to your basket")]

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        public class EnsureMinimumElementsAttribute : ValidationAttribute
        {
            private readonly int _min;
            private readonly int _max;

            public EnsureMinimumElementsAttribute(int min = 0, int max = int.MaxValue)
            {
                _min = min;
                _max = max;
            }

            public override bool IsValid(object value)
            {
                if (!(value is IList list))
                    return false;

                return list.Count >= _min && list.Count <= _max;
            }
        }

        public class ValidateEachItemAttribute : ValidationAttribute
        {
            protected readonly List<ValidationResult> validationResults = new List<ValidationResult>();

            public override bool IsValid(object value)
            {
                var list = value as IEnumerable;
                if (list == null) return true;

                var isValid = true;

                foreach (var item in list)
                {
                    var validationContext = new ValidationContext(item);
                    var isItemValid = Validator.TryValidateObject(item, validationContext, validationResults, true);
                    isValid &= isItemValid;
                }
                return isValid;
            }

            // I have ommitted error message formatting
        }

        /// <summary>
        ///  custom validation attribute ValueInListAttribute that accepts a list of allowed values in its constructor.
        ///  The IsValid method checks if the value is not null, converts it to a string, and then verifies 
        ///  whether it's present in the list of allowed values. If not, a validation error message is returned.
        ///  [ValueInList("AB", "AC", ErrorMessage = "Value must be 'AB' or 'AC'")]
        /// </summary>
        public class ValueInListAttribute : ValidationAttribute
        {
            private readonly string[] _allowedValues;

            public ValueInListAttribute(params string[] allowedValues)
            {
                _allowedValues = allowedValues;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value != null)
                {
                    string stringValue = value.ToString();

                    if (!_allowedValues.Contains(stringValue))
                    {
                        return new ValidationResult(ErrorMessage);
                    }
                }

                return ValidationResult.Success;
            }
        }
        /// <summary>
        /// Validates the allowed file extensions for an uploaded file.
        /// </summary>
        public class AllowedExtensionsAttribute : ValidationAttribute
        {
            private readonly string[] _extensions;

            /// <summary>
            /// Initializes a new instance of the <see cref="AllowedExtensionsAttribute"/> class.
            /// </summary>
            /// <param name="extensions">The allowed file extensions (including the dot, e.g., ".jpg", ".jpeg").</param>
            public AllowedExtensionsAttribute(params string[] extensions)
            {
                _extensions = extensions;
            }

            /// <inheritdoc />
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is IFormFile file)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension != null && !_extensions.Contains(extension.ToLower()))
                    {
                        return new ValidationResult($"Only {string.Join(", ", _extensions)} file extensions are allowed.");
                    }
                }

                return ValidationResult.Success;
            }
        }
        /// <summary>
        /// Validates the size of an uploaded file.
        /// </summary>

    }
}
