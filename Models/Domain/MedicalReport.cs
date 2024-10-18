namespace Health_Hub.Models.Domain
{
    public class MedicalReport
    {
        public int ReportID { get; set; }
        public string ReportDescription { get; set; }
        public string ReportDocument { get; set; }

        // Foreign key for Appointment
        public int AppointmentID { get; set; }
        public Appointment Appointment { get; set; }


    }
}
