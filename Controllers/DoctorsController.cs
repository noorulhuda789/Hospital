using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Data;
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
        [HttpGet]
        public async Task<IActionResult> Doctors(string filterType, string filterValue)
        {
            // Fetch all doctors with their specialization and hospital details
            var doctorsQuery = _context.Doctors
                .Include(d => d.Person)
                .Include(d => d.Specialization)
                .Include(d => d.DoctorHospitals)
                    .ThenInclude(dh => dh.Hospital)
                .AsQueryable();

            // Apply filter if filterType and filterValue are provided
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

            // Execute the query to get the list of doctors based on the filtering
            var doctors = await doctorsQuery.ToListAsync();

            // Fetch all specializations for the dropdown
            var specializations = _context.Lookups
                .Where(l => l.Category == "Specialization")
                .Select(s => new Lookup
                {
                    Category = s.Category,
                    Value = s.Value
                })
                .ToList();

            // Create a tuple model to pass both doctors and specializations
            var tupleModel = new Tuple<List<Doctor>, List<Lookup>>(doctors, specializations);

            return View(tupleModel);
        }
    }
}
