using Health_Hub.Data;
using Health_Hub.Models.Domain;
using Health_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Health_Hub.Controllers
{
    public class UpdatePasswordController : Controller
    {
        private readonly HealthHubDbContext _context;
        public UpdatePasswordController(HealthHubDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(PasswordUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Retrieve PersonID and RoleID from cookies
            int personId = int.Parse(Request.Cookies["PersonID"]);
            int roleId = int.Parse(Request.Cookies["RoleID"]); // Assuming you have RoleID in cookies

            // Fetch the relevant user (Patient or Doctor) based on RoleID and PersonID
            object user;
            if (roleId == 6) // Assuming 6 represents Doctor
            {
                user = await _context.Doctors
                    .Where(d => d.PersonID == personId)
                    .FirstOrDefaultAsync();
            }
            else if (roleId == 5) // Assuming 5 represents Patient
            {
                user = await _context.Patients
                    .Where(p => p.PersonID == personId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                // Role not recognized
                return RedirectToAction("Login", "Account");
            }

            if (user == null)
            {
                // User not found, redirect to login page
                return RedirectToAction("Login", "Account");
            }

            // Check if the current password matches
            bool passwordMatches = false;
            if (user is Doctor doctor)
            {
                passwordMatches = (model.CurrentPassword == doctor.Password); // Plain text comparison
            }
            else if (user is Patient patient)
            {
                passwordMatches = (model.CurrentPassword == patient.Password); // Plain text comparison
            }

            if (!passwordMatches)
            {
                ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                return View(model);
            }

            // If the current password matches, update to the new password
            if (user is Doctor doctorToUpdate)
            {
                doctorToUpdate.Password = model.NewPassword; // Storing plain text password
                _context.Update(doctorToUpdate);
            }
            else if (user is Patient patientToUpdate)
            {
                patientToUpdate.Password = model.NewPassword; // Storing plain text password
                _context.Update(patientToUpdate);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction("AccountDetails");
        }

    }
}
