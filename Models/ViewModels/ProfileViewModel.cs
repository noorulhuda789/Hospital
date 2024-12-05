using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Health_Hub.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be a 13-digit number.")]
        public string CNIC { get; set; }

        [Required(ErrorMessage = "Hospital name is required.")]
        [StringLength(100, ErrorMessage = "Hospital name cannot be longer than 100 characters.")]
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public string Capacity { get; set; }

        [Required(ErrorMessage = "Specialization is required.")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [GreaterThan(nameof(StartTime), ErrorMessage = "End time must be after start time.")]
        public TimeSpan EndTime { get; set; }

        // BreakTime and BreakEndTime are optional, but if provided they must fall within StartTime and EndTime range
        [OptionalWithinRange(nameof(StartTime), nameof(EndTime), ErrorMessage = "Break time must be within working hours.")]
        public TimeSpan? BreakTime { get; set; }

        [OptionalWithinRange(nameof(StartTime), nameof(EndTime), ErrorMessage = "Break end time must be within working hours.")]
        [GreaterThan(nameof(BreakTime), ErrorMessage = "Break end time must be after break start time.")]
        public TimeSpan? BreakEndTime { get; set; }

        [Required(ErrorMessage = "Day1 is required.")]
        public string Day1 { get; set; }

        [Required(ErrorMessage = "Day2 is required.")]
        public string Day2 { get; set; }

        [Required(ErrorMessage = "Profile image is required.")]
        public IFormFile ProfileImage { get; set; }

        [Required(ErrorMessage = "Degree image is required.")]
        public IFormFile DegreeImage { get; set; }

        public string DoctorHospitalDays
        {
            get
            {
                if (!string.IsNullOrEmpty(Day1) && !string.IsNullOrEmpty(Day2))
                {
                    return Day1.Substring(0, 3).ToUpper() + "-" + Day2.Substring(0, 3).ToUpper();
                }
                return string.Empty;
            }
        }
    }

    // Custom validation attribute to check if one TimeSpan is greater than another
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public GreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var currentValue = (TimeSpan)value;
                var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

                if (property == null)
                    throw new ArgumentException("Property with this name not found");

                var comparisonValue = (TimeSpan?)property.GetValue(validationContext.ObjectInstance);

                if (comparisonValue.HasValue && currentValue <= comparisonValue)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }

    // Custom validation attribute to ensure optional TimeSpan is within a specific range
    public class OptionalWithinRangeAttribute : ValidationAttribute
    {
        private readonly string _startProperty;
        private readonly string _endProperty;

        public OptionalWithinRangeAttribute(string startProperty, string endProperty)
        {
            _startProperty = startProperty;
            _endProperty = endProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) // If value is null, skip validation (optional field)
                return ValidationResult.Success;

            var currentValue = (TimeSpan)value;
            var startProperty = validationContext.ObjectType.GetProperty(_startProperty);
            var endProperty = validationContext.ObjectType.GetProperty(_endProperty);

            if (startProperty == null || endProperty == null)
                throw new ArgumentException("Properties with these names not found");

            var startValue = (TimeSpan?)startProperty.GetValue(validationContext.ObjectInstance);
            var endValue = (TimeSpan?)endProperty.GetValue(validationContext.ObjectInstance);

            if (startValue.HasValue && endValue.HasValue)
            {
                if (currentValue < startValue || currentValue > endValue)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
