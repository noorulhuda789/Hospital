using Health_Hub.Data;
using Health_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Health_Hub.Controllers
{
    public class DoctorDashboardController : Controller
    {
        private readonly HealthHubDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DoctorDashboardController(HealthHubDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult dashboard()
        {
            if (!Request.Cookies.ContainsKey("PersonId"))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if no PersonId
            }

            int personId;
            if (!int.TryParse(Request.Cookies["PersonId"], out personId))
            {
                return RedirectToAction("Login", "Account");
            }



			var doctorData = (from person in _context.People
                              join doctor in _context.Doctors on person.PersonID equals doctor.PersonId
                              where person.PersonID == personId
                              select new DoctorDashboardViewModel
                              {
                                  DoctorName = person.Name,
                                  // Count the total appointments by checking the DoctorHospital table
                                  TotalAppointments = _context.Appointments
                                      .Count(a => _context.DoctorHospitals
                                          .Any(dh => dh.DoctorID == doctor.PersonID)),
                                  // Fetch the rating for the doctor whose PersonID matches
                                  AverageRating = _context.Doctors
                                      .Where(d => d.PersonId == personId)
                                      .Select(d => d.Rating)
                                      .FirstOrDefault(), // No need for ?? if Rating is already a float
                                                         // Group appointments by date
                                  AppointmentsByDate = _context.Appointments
                                      .Where(a => _context.DoctorHospitals
                                          .Any(dh => dh.DoctorID == doctor.PersonID))
                                      .GroupBy(a => a.TimeCreated.Date)
                                      .Select(g => new AppointmentsPerDate
                                      {
                                          Date = g.Key,
                                          Count = g.Count()
                                      }).ToList()
                              }).FirstOrDefault();




            if (doctorData == null)
            {
                return NotFound("Doctor data not found.");
            }

			ViewData["Layout"] = "_LayoutDoctorLogIn";
			return View("~/Views/Doctors/dashBoard.cshtml", doctorData);
		}
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			int personId = int.Parse(Request.Cookies["PersonID"]!);
			try
			{
				if (Request.Cookies.ContainsKey("PersonID"))
				{
					

					if (personId != null)
					{
						// Query the doctor’s profile image from the database
						var doctor = _context.Doctors.FirstOrDefault(d => d.PersonID == personId);

						if (doctor != null)
						{
							// Check if the ProfileImage is null or empty before assigning to ViewBag
							ViewBag.DoctorImage = !string.IsNullOrEmpty(doctor.ProfileImage)
								? doctor.ProfileImage
								: "../Images/user.jpg"; // Default image if profile image is null or empty
						}
						else
						{
							// If no doctor is found, use a default image
							ViewBag.DoctorImage = "../Images/user.jpg";
						}
					}
					else
					{
						// If personId is null or parsing fails, use a default image
						ViewBag.DoctorImage = "../Images/user.jpg";
					}
				}
				else
				{
						// If no PersonID cookie exists, use a default image
						ViewBag.DoctorImage = "../Images/user.jp";
				}
			}
			catch (FormatException ex)
			{
				// Handle format exceptions, such as invalid integer parsing
				ViewBag.DoctorImage = "../Images/user.jpg";  // Use default image
																	// Log the exception if necessary
				Console.WriteLine($"Error parsing PersonID: {ex.Message}");
			}
			catch (Exception ex)
			{
				// Catch any other exceptions and log them
				ViewBag.DoctorImage = "../Images/user.jpg";  // Use default image
																	// Log the exception
				Console.WriteLine($"An error occurred: {ex.Message}" + personId);
			}

			base.OnActionExecuting(context);
		}



	}
}
