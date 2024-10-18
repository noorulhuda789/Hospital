namespace Health_Hub.Models.Domain
{
    public class DoctorHospital
    {
        public int DoctorHospitalID { get; set; }

        public int DoctorID { get; set; }
        public Doctor Doctor { get; set; }

        public int HospitalID { get; set; }
        public Hospital Hospital { get; set; }

        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
        public string WeekDays { get; set; }
        public int Capacity { get; set; }
    }
}
