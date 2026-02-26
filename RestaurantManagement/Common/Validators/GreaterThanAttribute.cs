using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Common.Validators
{
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public GreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (TimeSpan)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (TimeSpan)property.GetValue(validationContext.ObjectInstance)!;

            if (currentValue <= comparisonValue)
                return new ValidationResult(ErrorMessage ?? $"Must be greater than {_comparisonProperty}");

            return ValidationResult.Success;
        }
    }
}
