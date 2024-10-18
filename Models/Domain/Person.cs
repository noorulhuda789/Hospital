namespace Health_Hub.Models.Domain
{
    public class Person
    {
        public int PersonID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long CNIC { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        
        // Role (Doctor, Patient)
        public int RoleID { get; set; }
        public Lookup Role { get; set; }

        // Notifications (One-to-Many)
        public ICollection<Notification> Notifications { get; set; }
    }
}
