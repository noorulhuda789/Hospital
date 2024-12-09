using System;
using System.ComponentModel.DataAnnotations;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Build.ObjectModelRemoting;

namespace Health_Hub.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int PersonId { get; set; }

        // Name validation
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        // CNIC validation
        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be a 13-digit number.")]
        public string CNIC { get; set; }

        // Specialization validation
        [Required(ErrorMessage = "Specialization is required.")]
        public int SpecializationId { get; set; }

        // Profile image validation
        [Required(ErrorMessage = "Profile image is required.")]
        [DataType(DataType.Upload)]
        public IFormFile ProfileImage { get; set; }

        // Degree image validation
        [Required(ErrorMessage = "Degree image is required.")]
        [DataType(DataType.Upload)]
        public IFormFile DegreeImage { get; set; }

        // Phone number validation
        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone Number must be an 11-digit number.")]
        public string PhoneNumber { get; set; }

        // Email validation
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        // Specialization name display
        public string Specialization { get; set; }

        // Specialization name from Lookup table (if available)
        public string SpecializationName { get; set; }

        // Degree (for the display of the degree image path)
        public string Degree { get; set; }

        // Profile image (for displaying the profile image path)
        public string Profile { get; set; }
        public List<Lookup> Specializations { get; set; }
        // List of availabilities
        public List<DoctorAvailability> DoctorAvailabilities { get; set; }
    }

    // Doctor Availability Model
    public class DoctorAvailability
    {
        // Doctor name validation (optional)
        public string DoctorName { get; set; }

        // Hospital name validation
        [Required(ErrorMessage = "Hospital Name is required.")]
        public string HospitalName { get; set; }

        // Hospital address validation
        [Required(ErrorMessage = "Hospital Address is required.")]
        public string HospitalAddress { get; set; }

        // City validation
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        // Capacity validation
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }

        // Time start validation (required)
        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan TimeStart { get; set; }

        // Time end validation (required)
        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan TimeEnd { get; set; }

        // Break start validation (optional)
        public TimeSpan? BreakStart { get; set; }

        // Break end validation (optional)
        public TimeSpan? BreakEnd { get; set; }

        // Weekdays validation
        [Required(ErrorMessage = "Weekdays are required.")]
        [StringLength(7, ErrorMessage = "Weekdays should be in a valid format (e.g., Mon-Fri).")]
        public string WeekDays { get; set; }

        // Additional fields for day range (optional but validated)
        [Required(ErrorMessage = "Day 1 is required.")]
        public string day1 { get; set; }

        [Required(ErrorMessage = "Day 2 is required.")]
        public string day2 { get; set; }

    }

    // Model for updating week days for a doctor
    public class WeekDaysUpdateModel
    {
        [Required(ErrorMessage = "Hospital Name is required.")]
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Updated Weekdays are required.")]
        [StringLength(7, ErrorMessage = "Weekdays should be in a valid format (e.g., Mon-Fri).")]
        public string UpdatedWeekDays { get; set; }
    }
}
