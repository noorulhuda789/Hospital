using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Health_Hub.Data;

namespace Health_Hub.Controllers
{
	public class DoctorLayoutController : Controller
	{
		private readonly HealthHubDbContext _context;

		public DoctorLayoutController(HealthHubDbContext context)
		{
			_context = context;
		}

		// Ensure profile image is set before each action
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (Request.Cookies.ContainsKey("PersonID"))
			{
				int personId;
				if (int.TryParse(Request.Cookies["PersonID"], out personId))
				{
					// Query the doctor's profile image from the database
					var doctor = _context.Doctors.FirstOrDefault(d => d.PersonID == personId);
					if (doctor != null && !string.IsNullOrEmpty(doctor.ProfileImage))
					{
						ViewBag.DoctorImage = doctor.ProfileImage; // Set profile image URL
					}
					else
					{
						ViewBag.DoctorImage = Url.Content("~/Images/default-user.jpg"); // Default image
					}
				}
				else
				{
					ViewBag.DoctorImage = Url.Content("~/Images/default-user.jpg"); // Default image
				}
			}
			else
			{
				ViewBag.DoctorImage = Url.Content("~/Images/default-user.jpg"); // Default image
			}

			base.OnActionExecuting(context);
		}

		public IActionResult Index()
		{
			return View(); // Ensure your Index view is set up properly
		}
	}
}
