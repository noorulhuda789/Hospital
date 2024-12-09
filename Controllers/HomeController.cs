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
			// Check if the cookies exist
			if (Request.Cookies["RoleID"] != null || Request.Cookies["PersonID"] != null)
			{
				// Expire the cookies
				Response.Cookies.Delete("RoleID");
				Response.Cookies.Delete("PersonID");
			}



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
        public async Task<IActionResult> PatientSide()
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
            ViewData["Layout"] = "_LayoutLogInPatient";
            return View("Index");
        }
		public async Task<IActionResult> DoctorSide()
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
			ViewData["Layout"] = "_LayoutDoctorLogIn";
			return View("Index");
		}


		public IActionResult AboutUs()
        {
			string roleIdValue = Request.Cookies["RoleID"];

			if (roleIdValue == "5")
			{
				ViewData["Layout"] = "_LayoutLogInPatient";
				return View("AboutUs");
			}
			else if (roleIdValue == "6")
			{
				ViewData["Layout"] = "_LayoutDoctorLogIn";
				return View("AboutUs");
			}
			else if (string.IsNullOrEmpty(roleIdValue))
			{
				ViewData["Layout"] = "_Layout";
				return View("AboutUs");
			}
			return RedirectToAction("LogIn", "LogIn");
		}

        public IActionResult IndexForPatient()
        {
            ViewData["Layout"] = "_LayoutLogInPatient";


			string personIdValue = Request.Cookies["PersonID"];

			if (!string.IsNullOrEmpty(personIdValue))
			{
				// Convert the string to the appropriate type (if necessary)
				int personId = int.Parse(personIdValue);
				if (personId > 0)
				{
					var user = _context.People.FirstOrDefault(p => p.PersonID == personId);
					if (user != null)
					{
						ViewBag.User = user;
						PatientSide();
						return View("Index"); // Pass user to the view
					}
				}
			}

            return RedirectToAction("LogIn", "LogIn"); // Redirect if no valid PersonId
        }

        public IActionResult IndexForDoctor()
        {
            ViewData["Layout"] = "_LayoutDoctorLogIn";

            string personIdValue = Request.Cookies["PersonID"];

            if (!string.IsNullOrEmpty(personIdValue))
            {
                // Convert the string to the appropriate type (if necessary)
                int personId = int.Parse(personIdValue);
                if (personId > 0)
                {
                    var user = _context.People.FirstOrDefault(p => p.PersonID == personId);
                    if (user != null)
                    {
                        ViewBag.User = user;
                        DoctorSide();
                        return View("Index"); // Pass user to the view
                    }
                }
            }
            return RedirectToAction("LogIn", "LogIn");
        }


        public async Task<IActionResult> DoctorSide()
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
            ViewData["Layout"] = "_LayoutDoctorLogIn";
            return View("Index");
        }

        public async Task<IActionResult> Notifications( DateTime? date = null, string keyword = null)
        {
			string personIdValue = Request.Cookies["PersonID"];
            string roleIdValue = Request.Cookies["RoleID"];

            if (!string.IsNullOrEmpty(personIdValue))
            {
                // Convert the string to the appropriate type (if necessary)
                int personId = int.Parse(personIdValue);
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


                if (roleIdValue == "5")
                {
                    ViewData["Layout"] = "_LayoutLogInPatient";  
                }
                else if (roleIdValue == "6")
                {
                    ViewData["Layout"] = "_LayoutDoctorLogInPatient";
                }
                return View(); // The view name is inferred, so "View()" is sufficient.
            }
			return RedirectToAction("LogIn", "LogIn"); // Redirect if no valid PersonId
		}
    }
}
