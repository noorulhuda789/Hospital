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
			return LogIn();
		}
		[HttpPost]
		public async Task<IActionResult> Login(string Email, string Password)
		{
			// Check if the user exists in the database
			var user = await _context.People
				.FirstOrDefaultAsync(p => p.Email == Email && p.Password == Password);

			if (user != null)
			{
				// Return PersonID if credentials are valid
				return Json(new { success = true, personId = user.PersonID });
			}

			// If the user is not found, return an error message
			return Json(new { success = false, message = "Email or password doesn't exist." });
		}

	}
}
