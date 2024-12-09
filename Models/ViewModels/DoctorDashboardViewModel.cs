namespace Health_Hub.Models.ViewModels
{
    public class DoctorDashboardViewModel
    {
        public string DoctorName { get; set; }
        public int TotalAppointments { get; set; }
        public float AverageRating { get; set; }
        public List<AppointmentsPerDate> AppointmentsByDate { get; set; }
    }

    public class AppointmentsPerDate
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
