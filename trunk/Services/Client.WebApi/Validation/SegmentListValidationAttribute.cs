using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Client.WebApi.Validation
{
    public class SegmentListValidationAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _validSegments;
        public SegmentListValidationAttribute()
        {
            // Define the base valid segments
            _validSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "All", "EQUITY", "FNO_STOCK", "FNO_INDEX", "COMMODITY"
            };
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Segment is required.");

            string input = value.ToString().Trim();

            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult("Segment is required.");

            // Split by commas
            var parts = input.Split(',')
                             .Select(x => x.Trim())
                             .ToList();

            // Check duplicates
            if (parts.Count != parts.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                return new ValidationResult("Duplicate segments are not allowed.");

            // Validate each part
            foreach (var part in parts)
            {
                if (!_validSegments.Contains(part))
                {
                    return new ValidationResult($"Invalid segment: {part}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
