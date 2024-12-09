namespace Health_Hub.Models.Domain
{
    public class Appointment
    {
        public int AppointmentID { get; set; }

        // Many-to-One with Patient
        public int PatientID { get; set; }
        public Patient Patient { get; set; }

        // Adding DoctorID for direct access
        public int DoctorID { get; set; }
        public Doctor Doctor { get; set; }

        // Many-to-One with DoctorHospital

        public int SelectedDoctorHospitalID { get; set; }
        public DoctorHospital DoctorHospital { get; set; }

        // Status as lookup reference
        public int StatusID { get; set; }
        public Lookup Status { get; set; }

        public DateTime TimeCreated { get; set; }
        public DateTime TimeSlot { get; set; }
        public string Prescriptions { get; set; } = "null";
        public string TestSuggested { get; set; } = "null";

        public ICollection<MedicalReport>? MedicalReports { get; set; }


    }
}
