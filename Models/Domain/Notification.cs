namespace Health_Hub.Models.Domain
{
    public class Notification
    {
        public int NotificationID { get; set; }

        // Foreign key
        public int ReceiverID { get; set; }
        public Person Receiver { get; set; }

        public string Message { get; set; }
        public DateTime TimeSent { get; set; }
    }
}
