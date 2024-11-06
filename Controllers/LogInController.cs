using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Models.Domain;
using Health_Hub.Models;
using Health_Hub.Data;
namespace Health_Hub.Controllers
{
	public class LogInController : Controller
	{
		private readonly HealthHubDbContext _context;
		public LogInController(HealthHubDbContext context)
		{
			_context = context;
		}
		public IActionResult LogIn()
		{
			return View();
		}
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            // Check if the user exists in the database
            var user = await _context.People
                .FirstOrDefaultAsync(p => p.Email == Email && p.Password == Password);

            if (user != null)
            {
                // Navigate to specific layout based on RoleID
                if (user.RoleID == 3)
                {
                    // Redirect to Patient layout
                    return RedirectToAction("IndexForPatient", "Home");
                }
                else if (user.RoleID == 4)
                {
                    // Redirect to Doctor layout
                    return RedirectToAction("IndexForDoctor", "Home");
                }
                else
                {
                    // Add more RoleID checks if needed
                    return RedirectToAction("Index", "Home");
                }
            }

            // If the user is not found, display error message in the view
            ViewData["ErrorMessage"] = "Email or password doesn't exist.";
            return View();
        }


    }
}
