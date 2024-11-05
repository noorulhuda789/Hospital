using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Health_Hub.Controllers
{
	public class DoctorsController : Controller
	{
		private readonly HealthHubDbContext _context;
		public DoctorsController(HealthHubDbContext context)
		{
			_context = context;
		}
		//Fetch the list of project ideas and pass it to the view
		public async Task<IActionResult> Doctors()
		{
			var doctors = await _context.Doctors
		   .Include(d => d.Person)
		   .Include(d => d.Specialization)
		   .ToListAsync();

			var specializations = _context.Lookups
				.Where(l => l.Category == "Specialization")
				.Select(s => new Lookup
				{
					Category = s.Category,
					Value = s.Value
				})
				.ToList();

			var tupleModel = new Tuple<List<Doctor>, List<Lookup>>(doctors, specializations);

			return View(tupleModel);
		}
	}
}
