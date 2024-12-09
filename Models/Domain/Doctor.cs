using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Health_Hub.Models.Domain
{
    public class Doctor : Person
    {
        public int PersonId { get; set; }
        public string? Degree { get; set; } //PMDC Verfication based on Degree
        public bool VerificationStatus { get; set; } = false;
        public float Rating { get; set; }
        public string? ProfileImage { get; set; }
        public Person Person { get; set; }
        // Specialization as lookup reference (One-to-Many)
        public int SpecializationID { get; set; }
		public Lookup Specialization { get; set; }



		// Doctor-Hospital (Many-to-Many)
		public ICollection<DoctorHospital>? DoctorHospitals { get; set; }

        // Doctor-Appointment (One-to-Many)
        public ICollection<Appointment>? Appointments { get; set; }

        // Doctor-MedicalReport (One-to-Many)
        public ICollection<MedicalReport>? MedicalReports { get; set; }

        // Doctor-Chat (One-to-Many)
        public ICollection<Chat>? Chats { get; set; }

        // Navigation property to Person
         
    }
}
