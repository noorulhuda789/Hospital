using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Health_Hub.Models.Domain
{
    public class Patient : Person
    {
        public string Address { get; set; }

        // Patient-Appointment (One-to-Many)
        public ICollection<Appointment> Appointments { get; set; }


        // Patient-Chat (One-to-Many)
        public ICollection<Chat> Chats { get; set; }
    }
}
