using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections;

namespace Client.WebApi
{
    public class CustomValidators
    {
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
        public abstract class ConditionalValidationAttribute : ValidationAttribute //, IClientValidatable
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

            //public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
            //{
            //    var rule = new ModelClientValidationRule()
            //    {
            //        ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
            //        ValidationType = ValidationName,
            //    };
            //    string depProp = BuildDependentPropertyId(metadata, context as ViewContext);
            //    // find the value on the control we depend on; if it's a bool, format it javascript style    
            //    string targetValue = (this.TargetValue ?? "").ToString();
            //    if (this.TargetValue.GetType() == typeof(bool))
            //    {
            //        targetValue = targetValue.ToLower();
            //    }
            //    rule.ValidationParameters.Add("dependentproperty", depProp);
            //    rule.ValidationParameters.Add("targetvalue", targetValue);
            //    // Add the extra params, if any    
            //    foreach (var param in GetExtraValidationParameters())
            //    {
            //        rule.ValidationParameters.Add(param);
            //    }
            //    yield return rule;
            //}

            //private string BuildDependentPropertyId(ModelMetadata metadata, ViewContext viewContext)
            //{
            //    string depProp = viewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(this.DependentProperty);
            //    // This will have the name of the current field appended to the beginning, because the TemplateInfo's context has had this fieldname appended to it.    
            //    var thisField = metadata.PropertyName + "_";
            //    if (depProp.StartsWith(thisField))
            //    {
            //        depProp = depProp.Substring(thisField.Length);
            //    }
            //    return depProp;
            //}
       
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

        public class DoubleRangeIfAttribute : ConditionalValidationAttribute
        {
            private readonly double minimum;
            private readonly double maximum;
            protected override string ValidationName
            {
                get { return "doublerangeif"; }
            }
            public DoubleRangeIfAttribute(double minimum, double maximum, string dependentProperty, object targetValue)
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

        public class EnsureMinElementsAttribute : ValidationAttribute
        {
            private readonly int _minElements;
            public EnsureMinElementsAttribute(int minElements)
            {
                _minElements = minElements;
            }

            public override bool IsValid(object value)
            {
                var list = value as IList;
                if (list != null)
                {
                    return list.Count >= _minElements;
                }
                return false;
            }
        }

        public class ValidTextInput : ValidationAttribute
        {
            private readonly string validInputs;
            public ValidTextInput(string validInputs)
            {
                this.validInputs = validInputs;
            }
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var strings = value as IEnumerable<string>;
                foreach (var item in strings)
                {
                    if (string.IsNullOrEmpty(item))
                        return new ValidationResult("At least one ClientID is required");
                }
                return ValidationResult.Success;
            }
        }

        //[NoDuplicateClientIDs(ErrorMessage = "Duplicate ClientIDs are not allowed")]
        public class NoDuplicateClientIDsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var list = value as List<string>;
                if (list == null)
                {
                    return new ValidationResult("Invalid property type.");
                }

                if (list.Distinct().Count() != list.Count)
                {
                    return new ValidationResult(ErrorMessage);
                }

                return ValidationResult.Success;
            }
        }

        //[NoNullOrEmptyClientIDs(ErrorMessage = "ClientIDs cannot be null or empty")]
        public class NoNullOrEmptyClientIDsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var list = value as List<string>;
                if (list == null)
                {
                    return new ValidationResult("Invalid property type.");
                }

                if (list.Any(string.IsNullOrEmpty))
                {
                    return new ValidationResult(ErrorMessage);
                }

                return ValidationResult.Success;
            }
        }
    }
}
