using System.ComponentModel.DataAnnotations;

namespace Health_Hub.Models.Domain
{
	public class Person
	{
		public int PersonID { get; set; }

		[Required(ErrorMessage = "Name is required.")]
		[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "CNIC is required.")]
		[RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be 13 digits.")]
		public long CNIC { get; set; }

		[Required(ErrorMessage = "Password is required.")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Phone Number is required.")]
		[RegularExpression(@"^\d{11}$", ErrorMessage = "Phone Number must be 11 digits.")]
		public string PhoneNumber { get; set; }

		// Role (Doctor, Patient)
		public int RoleID { get; set; }
		public Lookup? Role { get; set; } // Optional, loaded after submission

		// Notifications (One-to-Many), initialize to prevent null references
		public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        // You can have a navigation property back to Doctor if needed
        public Doctor? Doctor { get; set; }
    }
}
