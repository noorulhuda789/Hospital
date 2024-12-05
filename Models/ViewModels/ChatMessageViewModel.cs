namespace Health_Hub.Models.ViewModels
{
    // ViewModel for displaying a chat message
    public class ChatMessageViewModel
    {
        public string SenderRole { get; set; } // "Doctor" or "Patient"
        public string Message { get; set; }
        public DateTime TimeSent { get; set; }
    }
}
