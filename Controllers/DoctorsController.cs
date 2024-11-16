using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Data;
using System.Threading.Tasks;
namespace Health_Hub.Controllers
{
	public class DoctorsController : Controller
	{
		private readonly HealthHubDbContext _context;
		public DoctorsController(HealthHubDbContext context)
		{
			_context = context;
		}


 
        [HttpGet]
		public async Task<IActionResult> Doctors(string filterType, string filterValue)
		{
			string roleIdValue = Request.Cookies["RoleID"];


			var doctorsQuery = _context.Doctors
				.Include(d => d.Person)
				.Include(d => d.Specialization)
				.Where(d => d.VerificationStatus == true)
				.Include(d => d.DoctorHospitals)
					.ThenInclude(dh => dh.Hospital)
				.AsQueryable();

			if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(filterValue))
			{
				if (filterType == "specialization")
				{
					doctorsQuery = doctorsQuery.Where(d => d.Specialization.Value.Contains(filterValue));
				}
				else if (filterType == "city")
				{
					doctorsQuery = doctorsQuery.Where(d => d.DoctorHospitals.Any(dh => dh.Hospital.City.Contains(filterValue)));
				}
			}

			var doctors = await doctorsQuery.ToListAsync();
			var specializations = _context.Lookups
				.Where(l => l.Category == "Specialization")
				.Select(s => new Lookup
				{
					Category = s.Category,
					Value = s.Value
				})
				.ToList();

			var tupleModel = new Tuple<List<Doctor>, List<Lookup>>(doctors, specializations);

			if (roleIdValue == "5")
			{
				ViewData["Layout"] = "_LayoutLogInPatient";
			}
			else if (roleIdValue == "6")
			{
				ViewData["Layout"] = "_LayoutDoctorLogIn";
			}
			else
			{
				ViewData["Layout"] = "_Layout";
			}

			return View(tupleModel);
		}

		// GET: Doctors/DoctorSingle
		[HttpGet]
		public async Task<IActionResult> DoctorSingle(int? id)
		{

			var doctor = await _context.Doctors
				.Include(d => d.Person)
				.Include(d => d.DoctorHospitals!)
					.ThenInclude(dh => dh.Hospital)
				.Include(d => d.Specialization)
				.FirstOrDefaultAsync(m => m.PersonID == id);

			if (doctor == null)
			{
				return NotFound();
			}

			// Pass the doctor and associated details to the view
			ViewData["Layout"] = "_LayoutLogInPatient";
			return View(doctor);
		}

	}
}
