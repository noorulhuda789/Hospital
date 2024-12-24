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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Health_Hub.Controllers
{
    public class DoctorProfileController : Controller
    {
        private readonly HealthHubDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IWebHostEnvironment _hostEnvironment;

        // Inject IWebHostEnvironment through constructor

        public DoctorProfileController(HealthHubDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

		// Display Doctor Profile
		public IActionResult DoctorProfile()
		{
			// Check if the "PersonId" cookie exists
			if (!Request.Cookies.ContainsKey("PersonId"))
			{
				return RedirectToAction("DoctorProfile"); // Redirect if PersonId cookie is missing
			}

			int personId;
			// Try parsing the PersonId cookie
			if (!int.TryParse(Request.Cookies["PersonId"], out personId))
			{
				return RedirectToAction("DoctorProfile"); // Redirect if parsing fails
			}

			// Fetch profile data
			var profileData = (from person in _context.People
							   join doctor in _context.Doctors on person.PersonID equals doctor.PersonId into doctorJoin
							   from doctor in doctorJoin.DefaultIfEmpty() // Left join to handle missing doctor records
							   join lookup in _context.Lookups on doctor.SpecializationID equals lookup.LookupID into lookupJoin
							   from lookup in lookupJoin.DefaultIfEmpty() // Left join to handle missing lookup records
							   where person.PersonID == personId
							   select new
							   {
								   person,
								   doctor,
								   lookup
							   }).FirstOrDefault();

			if (profileData == null)
			{
				// Redirect to login layout if no profile data is found
				ViewData["Layout"] = "../Shared/_LayoutDoctorLogIn";
				return View("~/Views/Doctors/DoctorProfile.cshtml", null);
			}

			// Manually handle nulls outside the query
			var model = new ProfileViewModel
			{
				PersonId = profileData.person.PersonID,
				Name = profileData.person.Name,
				CNIC = profileData.person.CNIC,
				PhoneNumber = profileData.person.PhoneNumber,
				Email = profileData.person.Email,
				Profile = profileData.doctor?.ProfileImage ?? null,
				SpecializationId = profileData.doctor?.SpecializationID ?? 0,
				SpecializationName = profileData.lookup?.Value,
				Degree = profileData.doctor?.Degree,
				DoctorAvailabilities = (profileData.doctor != null)
					? _context.DoctorHospitals
						.Where(dh => dh.DoctorID == profileData.doctor.PersonID && !dh.WeekDays.StartsWith("$"))
						.Select(dh => new DoctorAvailability
						{
							HospitalName = dh.Hospital.Name,
							HospitalAddress = dh.Hospital.Address,
							City = dh.Hospital.City,
							TimeStart = dh.TimeStart,
							TimeEnd = dh.TimeEnd,
							BreakStart = dh.BreakStart,
							BreakEnd = dh.BreakEnd,
							WeekDays = dh.WeekDays,
							Capacity = dh.Capacity
						}).ToList()
					: new List<DoctorAvailability>() // Empty list if doctor is null
			};
            if(model.Degree == null)
            {
				ViewData["Layout"] = "../Shared/_LayoutDoctorLogIn";
				return View("~/Views/Doctors/editProfile.cshtml", model);
			}
			// Return profile data view
			ViewData["Layout"] = "../Shared/_LayoutDoctorLogIn";
			return View("~/Views/Doctors/DoctorProfile.cshtml", model);
		}





		// POST: DoctorProfile/UploadImage
		[HttpPost]
        public IActionResult UploadImage(IFormFile file)
        {
            if (file != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                int personId = int.Parse(Request.Cookies["PersonId"]);
                var doctor = _context.Doctors.FirstOrDefault(d => d.PersonId == personId);
                if (doctor != null)
                {
                    // Update profile image path in database
                    doctor.ProfileImage = $"/images/{uniqueFileName}";
                    _context.SaveChanges();
                }

                // Return success response with the new image path
                return Json(new { success = true, imagePath = $"/images/{uniqueFileName}" });
            }

            return Json(new { success = false, message = "No file uploaded" });
        }

        // Helper method to save uploaded images
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return $"/images/{uniqueFileName}";
            }

            return null;
        }

        [HttpPost]
        public IActionResult UpdateWeekDays([FromBody] WeekDaysUpdateModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.HospitalName) || string.IsNullOrEmpty(model.UpdatedWeekDays))
            {
                return Json(new { success = false, message = "Invalid input data" });
            }

            var doctorHospital = _context.DoctorHospitals
                .Include(dh => dh.Hospital)
                .FirstOrDefault(dh => dh.Hospital.Name == model.HospitalName);

            if (doctorHospital != null)
            {
                doctorHospital.WeekDays = model.UpdatedWeekDays;
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Hospital not found" });
        }
        [HttpPost]
        public IActionResult AddAvailability(DoctorAvailability model)
        {
            // Validate model fields
            if (string.IsNullOrEmpty(model.HospitalName))
            {
                return Json(new { success = false, message = "Hospital Name is required." });
            }
            if (model.Capacity <= 0)
            {
                return Json(new { success = false, message = "Capacity must be greater than zero." });
            }
            if (model.TimeStart >= model.TimeEnd)
            {
                return Json(new { success = false, message = "Start Time must be earlier than End Time." });
            }
            if (string.IsNullOrEmpty(model.day1) || string.IsNullOrEmpty(model.day2))
            {
                return Json(new { success = false, message = "Start Day and End Day are required." });
            }

            // Retrieve the hospital based on the name
            var hospital = _context.Hospitals.FirstOrDefault(h => h.Name.ToLower() == model.HospitalName.ToLower());
            if (hospital == null)
            {
                return Json(new { success = false, message = "Hospital does not exist. Please add the hospital first." });
            }

            // Retrieve the doctor ID from cookies
            int personId;
            if (!int.TryParse(Request.Cookies["PersonId"], out personId))
            {
                return Json(new { success = false, message = "Invalid doctor ID. Please log in again." });
            }

            // Check for overlapping availability across all hospitals
            var overlappingAvailability = _context.DoctorHospitals
                .Any(dh => dh.DoctorID == personId &&
                           dh.WeekDays == $"{model.day1.Substring(0, 3).ToUpper()}-{model.day2.Substring(0, 3).ToUpper()}" &&
                           ((dh.TimeStart < model.TimeEnd && dh.TimeEnd > model.TimeStart) ||
                            (dh.TimeStart == model.TimeStart && dh.TimeEnd == model.TimeEnd)) &&
                           dh.HospitalID != hospital.HospitalID); // Exclude the current hospital

            if (overlappingAvailability)
            {
                return Json(new { success = false, message = "Overlapping availability detected for these weekdays and time range at another hospital." });
            }

            // Check if break time is within the start and end time
            if (model.BreakStart.HasValue && model.BreakEnd.HasValue)
            {
                if (model.BreakStart >= model.BreakEnd || model.BreakStart < model.TimeStart || model.BreakEnd > model.TimeEnd)
                {
                    return Json(new { success = false, message = "Break time must be within the start and end times." });
                }
            }

            // Add the availability
            var doctorHospital = new DoctorHospital
            {
                DoctorID = personId,
                HospitalID = hospital.HospitalID, // Use the retrieved hospital ID
                WeekDays = $"{model.day1.Substring(0, 3).ToUpper()}-{model.day2.Substring(0, 3).ToUpper()}",
                TimeStart = model.TimeStart,
                TimeEnd = model.TimeEnd,
                BreakStart = model.BreakStart,
                BreakEnd = model.BreakEnd,
                Capacity = model.Capacity
            };

            _context.DoctorHospitals.Add(doctorHospital);
            _context.SaveChanges();

            return Json(new { success = true, message = "Availability added successfully." });
        }



        [HttpPost]
        public async Task<IActionResult> UploadDegreeImage(IFormFile file)
        {
            if (file != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                int personId = int.Parse(Request.Cookies["PersonId"]);
                var doctor = _context.Doctors.FirstOrDefault(d => d.PersonId == personId);
                if (doctor != null)
                {
                    // Update profile image path in database
                    doctor.Degree = $"/images/{uniqueFileName}";
                    _context.SaveChanges();
                }

                // Return success response with the new image path
                return Json(new { success = true, imagePath = $"/images/{uniqueFileName}" });
            }
            return Json(new { success = false, message = "No file uploaded" });

        }
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                int personId = int.Parse(Request.Cookies["PersonId"]);
                var doctor = _context.Doctors.FirstOrDefault(d => d.PersonId == personId);
                if (doctor != null)
                {
                    // Update profile image path in database
                    doctor.ProfileImage = $"/images/{uniqueFileName}";
                    _context.SaveChanges();
                }

                // Return success response with the new image path
                return Json(new { success = true, imagePath = $"/images/{uniqueFileName}" });
            }
            return Json(new { success = false, message = "No file uploaded" });

        }
        [HttpPost]
        public IActionResult UpdatePersonalInfo(string Name, string PhoneNumber, string Email, string CNIC, int SpecializationId)
        {
            var personIdFromCookie = Request.Cookies["PersonId"];
            if (string.IsNullOrEmpty(personIdFromCookie) || !int.TryParse(personIdFromCookie, out int personId))
            {
                return Json(new { success = false, message = "Invalid or missing PersonId." });
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.PersonId == personId);
            if (doctor == null)
            {
                return Json(new { success = false, message = "Doctor not found." });
            }

            var person = _context.People.FirstOrDefault(p => p.PersonID == personId);
            if (person == null)
            {
                return Json(new { success = false, message = "Person not found." });
            }

            // Update personal details
            person.Name = Name;
            person.PhoneNumber = PhoneNumber;
            person.Email = Email;
            person.CNIC = CNIC;

            // Update specialization
            doctor.SpecializationID = SpecializationId;

            _context.SaveChanges();

            return Json(new { success = true, message = "Personal information updated successfully." });
        }


        public IActionResult EditPersonalInfo(int doctorId)
        {
            // Get the doctor from the database
            var doctor = _context.Doctors.FirstOrDefault(d => d.SpecializationID== doctorId);

            if (doctor == null)
            {
                // Handle case if doctor not found
                return NotFound();
            }

            // Get specializations from the database where category is "Specialization"
            var specializations = _context.Lookups
                                           .Where(s => s.Category == "Specialization")
                                           .ToList();

            // Create the view model with the list of specializations and the current specialization ID
            var model = new ProfileViewModel
            {
                Specializations = specializations,
                SpecializationId = doctor.SpecializationID// This is the current specialization ID of the doctor
            };

            return PartialView("_EditPersonalInfoModal", model); // Return the model to the view
        }

        
        [HttpPost]
        public IActionResult AddOrUpdateHospitalAvailability()
        {
            var hospitalName = Request.Form["HospitalName"].ToString();
            var city = Request.Form["City"].ToString();
            var hospitalAddress = Request.Form["HospitalAddress"].ToString();
            var day1 = Request.Form["Day1"].ToString();
            var day2 = Request.Form["Day2"].ToString();
            var timeStart = TryParseTime(Request.Form["TimeStart"].ToString());
            var timeEnd = TryParseTime(Request.Form["TimeEnd"].ToString());
            var breakStart = TryParseTime(Request.Form["BreakStart"].ToString());
            var breakEnd = TryParseTime(Request.Form["BreakEnd"].ToString());
            var capacity = Request.Form["Capacity"].ToString();

            // Ensure required fields are provided
            if (string.IsNullOrEmpty(hospitalName) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(hospitalAddress) ||
                string.IsNullOrEmpty(day1) || string.IsNullOrEmpty(day2) || !timeStart.HasValue || !timeEnd.HasValue)
            {
                return Json(new { success = false, message = "Please fill all required fields." });
            }
            if (timeStart > timeEnd)
            {
                return Json(new { success = false, message = " Start time must be before end time." });

            }
			if (breakStart.HasValue && breakEnd.HasValue)
			{
				if (breakStart > breakEnd)
				{
					return Json(new { success = false, message = "Break start time must be before break end time." });
				}

				if (breakStart < timeStart || breakEnd > timeEnd)
				{
					return Json(new { success = false, message = "Break time must be between the start and end times."+breakStart+timeStart+timeEnd+breakEnd });
				}
			}
            // Check if hospital exists or create a new one
            var hospital = _context.Hospitals.FirstOrDefault(h => h.Name == hospitalName);
            if (hospital == null)
            {
                hospital = new Hospital
                {
                    Name = hospitalName,
                    City = city,
                    Address = hospitalAddress
                };
                _context.Hospitals.Add(hospital);
                _context.SaveChanges();
            }
			// Validate break times


			int personId = int.Parse(Request.Cookies["PersonId"]);
            var doctorHospital = _context.DoctorHospitals
                .FirstOrDefault(dh => dh.DoctorID == personId && dh.HospitalID == hospital.HospitalID);

            if (doctorHospital == null)
            {
                doctorHospital = new DoctorHospital
                {
                    DoctorID = personId,
                    HospitalID = hospital.HospitalID,
                    WeekDays = $"{day1.Substring(0, 3).ToUpper()}-{day2.Substring(0, 3).ToUpper()}",
                    TimeStart = timeStart.Value,
                    TimeEnd = timeEnd.Value,
                    BreakStart = breakStart,
                    BreakEnd = breakEnd,
                    Capacity = int.Parse(capacity)
                };
                _context.DoctorHospitals.Add(doctorHospital);
            }
            else
            {
                // Update existing record
                doctorHospital.WeekDays = $"{day1.Substring(0, 3).ToUpper()}-{day2.Substring(0, 3).ToUpper()}";
                doctorHospital.TimeStart = timeStart.Value;
                doctorHospital.TimeEnd = timeEnd.Value;
                doctorHospital.BreakStart = breakStart;
                doctorHospital.BreakEnd = breakEnd;
                doctorHospital.Capacity = int.Parse(capacity);
            }



            // Save changes
            _context.SaveChanges();
            return Json(new { success = true, message = "Hospital and availability details added/updated successfully." });
        }

        private TimeSpan? TryParseTime(string time)
        {
            if (DateTime.TryParse(time, out DateTime parsedTime))
            {
                return parsedTime.TimeOfDay;
            }
            return null;
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
        [HttpPost]
        public JsonResult UpdateProfile(ProfileViewModel model)
        {
            try
            {
                


                int personId = int.Parse(Request.Cookies["PersonID"]!);

                // Find the existing person record in the database by PersonId
                var person = _context.People.FirstOrDefault(p => p.PersonID == personId);

                if (person != null)
                {
                    // Update the person data
                    person.Name = model.Name;
                    person.PhoneNumber = model.PhoneNumber;
                    person.Email = model.Email;
                    person.CNIC = model.CNIC;

                    _context.SaveChanges();

                    // Return success message
                    return Json(new { success = true, message = "Profile updated successfully!" });
                }

                // If person is not found
                return Json(new { success = false, message = "Person not found!" });
            }

            catch (Exception ex)
            {

                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }



    }
}
