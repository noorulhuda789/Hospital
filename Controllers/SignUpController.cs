using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Health_Hub.Controllers
{
	public class SignUpController : Controller
	{
		private readonly HealthHubDbContext _context;
		public SignUpController(HealthHubDbContext context)
		{
			_context = context;
		}
		public IActionResult SignUp()
		{
			// Assuming you have a DbContext called _context
			var roles = _context.Lookups
						.Where(l => l.Category == "Role")
						.Select(l => new SelectListItem
						{
							Value = l.LookupID.ToString(),  // This will be 1 for Patient, 2 for Doctor
							Text = l.Value                  // Display the role name (Patient, Doctor)
						}).ToList();

			ViewBag.RoleID = roles;

			return View();
		}
		[HttpPost]
		public async Task<IActionResult> SignUp(Person person)
		{
			if (ModelState.IsValid)
			{
				// Save the person details to the People table
				await _context.People.AddAsync(person);
				await _context.SaveChangesAsync(); // This saves the person and generates PersonID

				//// Create Patient or Doctor based on RoleID
				//if (person.RoleID == 1) // RoleID = 1 for Patient
				//{
				//		var patient = new Patient
				//		{
				//			PersonID = person.PersonID // Use the generated PersonID
				//		};
				//		await _context.SaveChangesAsync();
				//}
				//else if (person.RoleID == 2) // RoleID = 2 for Doctor
				//{
				//		var doctor = new Doctor
				//		{
				//			PersonID = person.PersonID // Use the generated PersonID
				//		};
				//		await _context.SaveChangesAsync();
				//}
				AddRoleSpecificEntity(person);
				// Redirect based on RoleID
				if (person.RoleID == 1)
				{
					return RedirectToAction("_LayoutLogInPatient", "Index");
				}
				else if (person.RoleID == 2)
				{
					return RedirectToAction("_LayoutDoctorLogIn", "Index");
				}
			}

			// If the model state is invalid, return the view with validation messages
			return View(person);
		}
		private async Task AddRoleSpecificEntity(Person person)
		{
			if (person.RoleID == 1) // RoleID = 1 for Patient
			{
				var patient = new Patient
				{
					PersonID = person.PersonID // Use the generated PersonID
				};
				await _context.Patients.AddAsync(patient);
			}
			else if (person.RoleID == 2) // RoleID = 2 for Doctor
			{
				var doctor = new Doctor
				{
					PersonID = person.PersonID // Use the generated PersonID
				};
				await _context.Doctors.AddAsync(doctor);
			}

			// Save the Patient or Doctor to the database
			await _context.SaveChangesAsync();
		}


	}

}
