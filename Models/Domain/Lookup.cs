namespace Health_Hub.Models.Domain
{
    public class Lookup
    {
        public int LookupID { get; set; }
        public string Category { get; set; }
        public string Value { get; set; }

        // One-to-Many relationships for different lookup uses
        public ICollection<Person> People { get; set; }
        public ICollection<Doctor> Doctors { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
        