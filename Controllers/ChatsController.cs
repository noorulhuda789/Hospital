using Health_Hub.Models.Domain;
using Health_Hub.Models.ViewModels;
using Health_Hub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Data;

namespace Health_Hub.Controllers
{
    public class ChatsController : Controller
    {
        private readonly HealthHubDbContext _context;

        public ChatsController(HealthHubDbContext context)
        {
            _context = context;
        }

        // GET: Chats (Unified Chat UI)
        public async Task<IActionResult> Chat()
        {
            string userId = Request.Cookies["PersonID"];
            string userRole = Request.Cookies["RoleID"];
            if (!int.TryParse(userId, out int id) || !int.TryParse(userRole, out int roleid))
            {
                return Unauthorized(); // Handle unauthorized access
            }

			var chatList = await _context.Chats
	        .Where(c => (roleid == 5 && c.PatientID == id) || (roleid == 6 && c.DoctorID == id))  // Filter based on role
	        .Include(c => c.Patient)  // Load related Patient data
	        .Include(c => c.Doctor)   // Load related Doctor data
	        .GroupBy(c => roleid == 5 ? c.DoctorID : c.PatientID)  // Group based on role: Patient -> DoctorID, Doctor -> PatientID
	        .Select(g => new ChatListItemViewModel
	        {
		        ChatID = g.FirstOrDefault().ChatID,
		        ParticipantID = roleid == 5 ? g.Key : g.FirstOrDefault().PatientID, // Determine Participant based on role
		        ParticipantName = roleid == 5 ? g.FirstOrDefault().Doctor.Name : g.FirstOrDefault().Patient.Name, // Get participant name
		        LastMessage = g.OrderByDescending(c => c.TimeSent).FirstOrDefault().Message,  // Get the last message
		        LastMessageTime = g.OrderByDescending(c => c.TimeSent).FirstOrDefault().TimeSent  // Get the time of the last message
	        })
	        .Where(chat => chat.LastMessage != null)  // Optional: Exclude chats without messages
	        .ToListAsync();



			// Set layout dynamically based on user role
			ViewData["Layout"] = roleid == 5 ? "_LayoutLogInPatient" : "_LayoutDoctorLogIn";
            return View(chatList);
        }

        public async Task<IActionResult> ChatList()
        {
            // Retrieve user ID and role
            int? userId = HttpContext.Session.GetInt32("PersonID");
            int? roleId = HttpContext.Session.GetInt32("RoleID");

            if (userId == null || roleId == null)
            {
                return BadRequest(new { message = "User ID or role is missing." });
            }

            var chats = await _context.Chats
                .Where(c => (c.PatientID == userId || c.DoctorID == userId)) // Filter by current user
                .Select(c => new ChatListItemViewModel
                {
                    ChatID = c.ChatID,
                    ParticipantID = c.PatientID == userId ? c.DoctorID : c.PatientID,
                    ParticipantName = c.PatientID == userId ? c.Doctor.Name : c.Patient.Name,
                    LastMessage = c.Message,
                    LastMessageTime = c.TimeSent
                })
                .OrderByDescending(c => c.LastMessageTime) // Optional: Order by most recent message
                .ToListAsync();

            // Set layout dynamically based on user role
            ViewData["Layout"] = roleId == 5 ? "_LayoutLogInPatient" : "_LayoutDoctorLogIn";
            return View(chats); // Pass the list to the view
        }

        public async Task<IActionResult> NewParticipants()
        {
            string userId = Request.Cookies["PersonID"];
            string userRoleid = Request.Cookies["RoleID"];
            if (!int.TryParse(userId, out int id) || !int.TryParse(userRoleid, out int roleid))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            var participants = roleid == 5 // If the user is a patient
                ? await _context.Appointments
                    .Where(a => a.PatientID == id)
                    .Select(a => new { ParticipantID = a.DoctorID, ParticipantName = a.Doctor.Name })
                    .Distinct()
                    .ToListAsync()
                : await _context.Appointments
                    .Where(a => a.DoctorID == id)
                    .Select(a => new { ParticipantID = a.PatientID, ParticipantName = a.Patient.Name })
                    .Distinct()
                    .ToListAsync();

            return Json(participants);
        }

        // GET: Chats/History/{id} (Fetch chat history with a participant)
        public async Task<IActionResult> History(int participantId)
        {
            string userId = Request.Cookies["PersonID"];
            string userRoleId = Request.Cookies["RoleID"];

            // Validate cookies
            if (!int.TryParse(userId, out int personId) || !int.TryParse(userRoleId, out int roleId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            // Fetch the chat history between the current user and the specific participant
            var chatHistory = await _context.Chats
                .Where(c =>
                    (c.PatientID == personId && c.DoctorID == participantId) || // Current user is Patient
                    (c.DoctorID == personId && c.PatientID == participantId))   // Current user is Doctor
                .OrderBy(c => c.TimeSent)
                .Select(c => new ChatMessageViewModel
                {
                    SenderRole = c.PatientID == personId ? "Patient" : "Doctor", // Identify sender
                    Message = c.Message,
                    TimeSent = c.TimeSent
                })
                .ToListAsync();

            // Check if any messages exist
            if (!chatHistory.Any())
            {
                return NotFound(new { message = "No chat history found between the users." });
            }

            return Json(chatHistory); // Return chat history as JSON
        }


        // POST: Chats/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage(int recipientId, string message)
        {
            string userId = Request.Cookies["PersonID"];
            string userRoleid = Request.Cookies["RoleID"];
            if (!int.TryParse(userId, out int personid) || !int.TryParse(userRoleid, out int roleid))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            string senderRole = userRoleid == "5" ? "Patient" : "Doctor";
            string recipientRole = senderRole == "Patient" ? "Doctor" : "Patient";

            var chat = new Chat
            {
                PatientID = roleid == 5 ? personid : recipientId,
                DoctorID = roleid == 6 ? personid : recipientId,
                Message = message,
                TimeSent = DateTime.Now,
                StatusID = 17 // Default to active or unread status
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // Get the sender's name and time
            var senderName = senderRole == "Patient" ? "You" : "Doctor";
            var timeSent = chat.TimeSent.ToString("hh:mm tt");

            var hubContext = HttpContext.RequestServices.GetService<IHubContext<ChatHub>>();
            await hubContext.Clients.User(recipientId.ToString()).SendAsync("ReceiveMessage", recipientId, message, timeSent, senderRole);

            return Ok(new { senderName, message, timeSent, senderRole });
        }


    }

}
