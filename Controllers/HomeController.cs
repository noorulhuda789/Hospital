using Health_Hub.Models;
using Health_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Health_Hub.Data;

namespace Health_Hub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HealthHubDbContext _context; // Add DbContext here

        public HomeController(ILogger<HomeController> logger, HealthHubDbContext context)
        {
            _logger = logger;
            _context = context; // Initialize the DbContext
        }

        public IActionResult Index()
        {
            // Query the Lookups table for departments with the category "Specialization"
            var departments = _context.Lookups
                .Where(l => l.Category == "Specialization")
                .Select(l => l.Value) // Assuming 'Value' holds the department name
                .ToList();

            ViewBag.Departments = departments; // Pass to the view using ViewBag

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
            Index();
            return View("Index"); // this statement will not be used only as the function has to return something so it is written here
        }

        public IActionResult IndexForDoctor()
        {
            ViewData["Layout"] = "_LayoutDoctorLogIn";// this function should be updated by noor as her need to navigate to doctor layout page
            return View("Index");// Same goes here as for indexforpatient
        }

        public IActionResult Login()
        {
            ViewData["Layout"] = "~/Views/Shared/LayoutLogInPatient.cshtml";
            return View();
        }

        public IActionResult AllDoctor()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
