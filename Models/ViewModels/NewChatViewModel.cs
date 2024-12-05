namespace Health_Hub.Models.ViewModels
{
    // ViewModel for starting a new chat
    public class NewChatViewModel
    {
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }
        public bool HasAppointment { get; set; } // Ensure patient has an appointment
    }
}
