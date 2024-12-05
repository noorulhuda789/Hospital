namespace Health_Hub.Models.ViewModels
{
    // ViewModel for displaying chat list items
    public class ChatListItemViewModel
    {
        public int ChatID { get; set; }
        public int ParticipantID { get; set; } // PatientID or DoctorID
        public string ParticipantName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
    }
}
