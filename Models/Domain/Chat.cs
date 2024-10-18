namespace Health_Hub.Models.Domain
{
    public class Chat
    {
        public int ChatID { get; set; }

        // Foreign keys
        public int PatientID { get; set; }
        public Patient Patient { get; set; }

        public int DoctorID { get; set; }
        public Doctor Doctor { get; set; }

        public string Message { get; set; }
        public DateTime TimeSent { get; set; }

        // Chat status as lookup reference
        public int StatusID { get; set; }
        public Lookup Status { get; set; }
    }
}
