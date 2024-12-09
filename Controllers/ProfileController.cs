using Health_Hub.Data;
using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Health_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult CompleteProfile()
        {
            ViewBag.Specializations = GetSpecializationSelectList();
            ViewData["Layout"] = "_LayoutDoctorLogIn";
            return View("~/Views/Doctors/completeProfile.cshtml");
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
            var person = await _context.People.FirstOrDefaultAsync(p => p.CNIC == model.CNIC && p.RoleID == 4);
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

    }
}