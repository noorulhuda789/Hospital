using Microsoft.AspNetCore.Http;
using Health_Hub.Models;
using Health_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Health_Hub.Data;
using Health_Hub.Models.Domain;
using Azure.Core;

namespace Health_Hub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HealthHubDbContext _context;

        public HomeController(ILogger<HomeController> logger, HealthHubDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var departments = _context.Lookups
                .Where(l => l.Category == "Specialization")
                .Select(l => l.Value)
                .ToList();

            ViewBag.Departments = departments;

            var doctors = _context.Doctors
                .Include(d => d.Specialization)
                .Where(d => d.VerificationStatus == true)
                .OrderByDescending(d => d.Rating)
                .Take(3)
                .Select(d => new DoctorVM
                {
                    PersonID = d.PersonID,
                    Name = d.Name,
                    ProfileImage = d.ProfileImage,
                    Specialization = d.Specialization.Value,
                    Rating = d.Rating
                })
                .ToList();

            ViewBag.TopDoctors = doctors;

            return View();
        }

        public IActionResult IndexForPatient()
        {
            ViewData["Layout"] = "_LayoutLogInPatient";

            // Retrieve the PersonId from TempData
            int personId = TempData["PersonId"] != null ? (int)TempData["PersonId"] : 0;

            if (personId > 0)
            {
                var user = _context.People.FirstOrDefault(p => p.PersonID == personId);
                if (user != null)
                {
                    ViewBag.User = user;
                    Index();
                    return View("Index"); // Pass user to the view
                }
            }

            return RedirectToAction("LogIn", "LogIn"); // Redirect if no valid PersonId
        }

        public IActionResult IndexForDoctor()
        {
            ViewData["Layout"] = "_LayoutDoctorLogIn";

            // Retrieve the PersonId from TempData
            int personId = TempData["PersonId"] != null ? (int)TempData["PersonId"] : 0;

            if (personId > 0)
            {
                var user = _context.People.FirstOrDefault(p => p.PersonID == personId);
                if (user != null)
                {
                    ViewBag.User = user;
                    Index();
                    return View("Index"); // Pass user to the view
                }
            }

            return RedirectToAction("LogIn", "LogIn");
        }

        public IActionResult Notifications(int personId, DateTime? date = null, string keyword = null)
        {
            var user = _context.People.FirstOrDefault(p => p.PersonID == personId); // Ensure user exists, or handle redirect
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Start with notifications of the person
            var notificationsQuery = _context.Notifications.Where(n => n.ReceiverID == personId).AsQueryable();

            // Filter by date if provided
            if (date.HasValue)
            {
                notificationsQuery = notificationsQuery
                    .Where(n => n.TimeSent.Date == date.Value.Date);
            }

            // Filter by keyword if provided
            if (!string.IsNullOrEmpty(keyword))
            {
                notificationsQuery = notificationsQuery
                    .Where(n => n.Message.Contains(keyword));
            }

            // Order by TimeSent descending to display latest notifications on top
            var notifications = notificationsQuery
                .OrderByDescending(n => n.TimeSent)
                .Take(20)
                .ToList();

            ViewBag.notifications = notifications;
            ViewBag.PersonId = personId;
            return View(); // The view name is inferred, so "View()" is sufficient.
        }
    }
}
