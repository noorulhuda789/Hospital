namespace Health_Hub.Models.Domain
{
    public class Hospital
    {
        public int HospitalID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        // Doctor-Hospital (Many-to-Many)
        public ICollection<DoctorHospital> DoctorHospitals { get; set; }
    }
}
