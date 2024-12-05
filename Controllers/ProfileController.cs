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
        public async Task<IActionResult> CompleteProfile(ProfileViewModel model)
        {
           
            if (!ModelState.IsValid)
            {
                ViewBag.Specializations = GetSpecializationSelectList();
                ViewData["Layout"] = "_LayoutDoctorLogIn";
                return View("~/Views/Doctors/completeProfile.cshtml", model);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {


                    // Create or update person
                    var person = await GetOrCreatePersonAsync(model);
                    // Save images and get paths
                    var profileImagePath = await SaveImageAsync(model.ProfileImage);
                    var degreeImagePath = await SaveImageAsync(model.DegreeImage);

                    // Create or update doctor and save images
                    var doctor = await GetOrCreateDoctorAsync(person.PersonID, model, profileImagePath, degreeImagePath);

                    // Link doctor with hospital, if necessary
                    var hospital = await GetOrCreateHospitalAsync(model);
                    await GetOrCreateDoctorHospitalAsync(doctor.PersonID, hospital.HospitalID, model);

                    // Commit transaction
                    await transaction.CommitAsync();

                    return RedirectToAction("ProfileSuccess");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    ViewBag.Specializations = GetSpecializationSelectList();
                    return View("~/Views/Doctors/completeProfile.cshtml", model);
                }
            }
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
            var person = await _context.People.FirstOrDefaultAsync(p => p.CNIC == model.CNIC && p.RoleID == 4) ;
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

        // Create or update hospital record
        private async Task<Hospital> GetOrCreateHospitalAsync(ProfileViewModel model)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Name == model.HospitalName && h.Address == model.Address && h.City == model.City);
            if (hospital == null)
            {
                hospital = new Hospital
                {
                    Name = model.HospitalName,
                    Address = model.Address,
                    City = model.City
                };
                _context.Hospitals.Add(hospital);
                await _context.SaveChangesAsync();
            }
            return hospital;
        }

        // Link doctor and hospital with specified details
        private async Task GetOrCreateDoctorHospitalAsync(int doctorId, int hospitalId, ProfileViewModel model)
        {

            var doctorHospital = await _context.DoctorHospitals
                .FirstOrDefaultAsync(dh => dh.DoctorID == doctorId && dh.HospitalID == hospitalId);
            string doctorHospitalDays = model.DoctorHospitalDays;
            if (doctorHospital == null)
            {
                doctorHospital = new DoctorHospital
                {
                    DoctorID = doctorId,
                    HospitalID = hospitalId,
                    TimeStart = model.StartTime,
                    TimeEnd = model.EndTime,
                    BreakStart = model.BreakTime,
                    BreakEnd = model.BreakEndTime,
                    Capacity = int.Parse(model.Capacity),
                    WeekDays = doctorHospitalDays
                };
                _context.DoctorHospitals.Add(doctorHospital);
            }
            else
            {
                doctorHospital.TimeStart = model.StartTime;
                doctorHospital.TimeEnd = model.EndTime;
                doctorHospital.BreakStart = model.BreakTime;
                doctorHospital.BreakEnd = model.BreakEndTime;
                doctorHospital.Capacity = int.Parse(model.Capacity);
                doctorHospital.WeekDays = doctorHospitalDays;
                _context.DoctorHospitals.Update(doctorHospital);
            }
            await _context.SaveChangesAsync();
        }
    }
}
