using Health_Hub.Data;
using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Health_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Health_Hub.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HealthHubDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(HealthHubDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: CompleteProfile view
        [HttpGet]
        public IActionResult CompleteProfile()
        {
            ViewBag.Specializations = GetSpecializationSelectList();
            ViewData["Layout"] = "../Shared/_LayoutDoctorLogIn";
            return View("~/Views/Doctors/completeProfile.cshtml");
        }
        /*[HttpPost]
        public IActionResult CompleteProfile(ProfileViewModel model, IFormFile ProfileImage, IFormFile DegreeImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Assuming the profile image and degree image upload logic is handled
                    var profileImagePath = string.Empty;
                    var degreeImagePath = string.Empty;

                    // Handle the profile image upload
                    if (ProfileImage != null)
                    {
                        profileImagePath = SaveImageAsync(ProfileImage).Result;
                    }

                    // Handle the degree image upload
                    if (DegreeImage != null)
                    {
                        degreeImagePath = SaveImageAsync(DegreeImage).Result;
                    }

                    int personId = int.Parse(Request.Cookies["PersonID"]);
                    var person = _context.People.FirstOrDefault(p => p.PersonID == personId && p.RoleID == 6);

                    if (person != null)
                    {
                        person.Name = model.Name;
                        person.CNIC = model.CNIC;
                        _context.SaveChanges();

                        var doctor = _context.Doctors.FirstOrDefault(d => d.PersonID == personId);

                        if (doctor != null)
                        {
                            doctor.ProfileImage = profileImagePath;
                            doctor.Degree = degreeImagePath;
                            doctor.SpecializationID = model.SpecializationId;
                            _context.SaveChanges();
                        }

                        // Return success message to the view
                        ViewData["SuccessMessage"] = "Profile updated successfully!";
                    }

                    // Repopulate the dropdown and other data in the view
                    ViewBag.Specializations = GetSpecializationSelectList();
                    return View(model);  // Re-populate form with the current values
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    ViewData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                    return View(model);  // Repopulate form with the current values
                }
            }
            else
            {
                // If validation fails, return the form with validation errors
                ViewBag.Specializations = GetSpecializationSelectList();
                return View(model);
            }
        }*/

        [HttpPost]
        public IActionResult CompleteProfile12(ProfileViewModel model, IFormFile ProfileImage, IFormFile DegreeImage)
        {
          
                try
                {
                    // Assuming the profile image and degree image upload logic is handled
                    var profileImagePath = string.Empty;
                    var degreeImagePath = string.Empty;

                    // Handle the profile image upload
                    if (ProfileImage != null)
                    {
                        profileImagePath = SaveImageAsync(ProfileImage).Result;
                    }

                    // Handle the degree image upload
                    if (DegreeImage != null)
                    {
                        degreeImagePath = SaveImageAsync(DegreeImage).Result;
                    }

                    int personId = int.Parse(Request.Cookies["PersonID"]);
                    var person = _context.People.FirstOrDefault(p => p.PersonID == personId && p.RoleID == 6);

                    if (person != null)
                    {
                        person.Name = model.Name;
                        person.CNIC = model.CNIC;
                        _context.SaveChanges();

                        var doctor = _context.Doctors.FirstOrDefault(d => d.PersonID == personId);

                        if (doctor != null)
                        {
                            doctor.ProfileImage = profileImagePath;
                            doctor.Degree = degreeImagePath;
                            doctor.SpecializationID = model.SpecializationId;
                            _context.SaveChanges();
                        }

                        // Return success message to the view
                        ViewData["SuccessMessage"] = "Profile updated successfully!";
                    }

                    // Repopulate the dropdown and other data in the view
                    ViewBag.Specializations = GetSpecializationSelectList();

                // Explicitly specify the path to the view in the 'Doctors' folder
                ViewData["Layout"] = "../Shared/_LayoutDoctorLogIn";
                return View("~/Views/Doctors/dashBoard.cshtml"); // Re-populate form with the current values
            }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    ViewData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                    return View("~/Views/Doctors/CompleteProfile12.cshtml");  // Repopulate form with the current values
                }
            
           
        }


        // POST: CompleteProfile form submission
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
                    doctor.ProfileImage = $"/images/{uniqueFileName}";
                    _context.SaveChanges();
                }

                return Json(new { success = true, imagePath = $"/images/{uniqueFileName}" });
            }

            return Json(new { success = false });
        }

        // Helper method to save uploaded images
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return $"/images/{uniqueFileName}";
            }
            return null;
        }

        // Get list of specializations for dropdown
        private SelectList GetSpecializationSelectList()
        {
            var specializations = _context.Lookups
                .Where(l => l.Category == "Specialization")
                .Select(l => new SelectListItem
                {
                    Value = l.LookupID.ToString(),
                    Text = l.Value
                })
                .ToList();
            return new SelectList(specializations, "Value", "Text");
        }

        // Create or update person record
        private async Task<Person> GetOrCreatePersonAsync(ProfileViewModel model)
        {
            int personId = int.Parse(Request.Cookies["PersonID"]!);
            var person = await _context.People.FirstOrDefaultAsync(p => p.PersonID == personId && p.RoleID == 6);
            if (person == null)
            {
                person = new Person { Name = model.Name, CNIC = model.CNIC };
                _context.People.Add(person);
            }
            else
            {
                person.Name = model.Name;
                _context.People.Update(person);
            }
            await _context.SaveChangesAsync();
            return person;
        }

        // Create or update doctor record
        private async Task<Doctor> GetOrCreateDoctorAsync(int personId, ProfileViewModel model, string profileImagePath, string degreeImagePath)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.PersonID == personId);
            if (doctor == null)
            {
                doctor = new Doctor
                {
                    PersonID = personId,
                    ProfileImage = profileImagePath,
                    Degree = degreeImagePath,
                    SpecializationID = model.SpecializationId
                };
                _context.Doctors.Add(doctor);
            }
            else
            {
                doctor.ProfileImage = profileImagePath;
                doctor.Degree = degreeImagePath;
                doctor.SpecializationID = model.SpecializationId;
                _context.Doctors.Update(doctor);
            }
            await _context.SaveChangesAsync();
            return doctor;
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