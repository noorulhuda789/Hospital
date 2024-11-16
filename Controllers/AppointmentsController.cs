using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Data;
using Health_Hub.Models.Domain;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Health_Hub.Controllers
{
	public class AppointmentsController : Controller
	{
		private readonly HealthHubDbContext _context;

		public AppointmentsController(HealthHubDbContext context)
		{
			_context = context;
		}

		// GET: Appointments
		//public async Task<IActionResult> List()
		//{
		//    var healthHubDbContext = _context.Appointments.Include(a => a.Doctor).Include(a => a.DoctorHospital).Include(a => a.Patient).Include(a => a.Status);
		//    return View(await healthHubDbContext.ToListAsync());
		//}
		public async Task<IActionResult> List(string status = "Approved")
		{
			var appointments = _context.Appointments
		   .Include(a => a.Doctor)
		   .Include(a => a.Patient)
		   .Include(a => a.DoctorHospital)
		   .Include(a => a.Status)
		   .AsQueryable(); // Ensures the entire query stays as IQueryable

			switch (status)
			{
				case "Pending":
					appointments = appointments.Where(a => a.Status.Value == "Pending");
					break;
				case "Completed":
					appointments = appointments.Where(a => a.Status.Value == "Completed");
					break;
				default: // Default is Approved for upcoming appointments
					appointments = appointments.Where(a => a.Status.Value == "Approved");
					break;
			}
			ViewData["Layout"] = "_LayoutLogInPatient";
			return View(await appointments.ToListAsync());
		}


		// GET: Appointments/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var appointment = await _context.Appointments
				.Include(a => a.Doctor)
				.Include(a => a.DoctorHospital)
				.Include(a => a.Patient)
				.Include(a => a.Status)
				.FirstOrDefaultAsync(m => m.AppointmentID == id);
			if (appointment == null)
			{
				return NotFound();
			}

			return View(appointment);
		}

		// GET: Appointments/Create
		public IActionResult Create()
		{
			ViewData["DoctorID"] = new SelectList(_context.Doctors, "PersonID", "CNIC");
			ViewData["SelectedDoctorHospitalID"] = new SelectList(_context.DoctorHospitals, "DoctorHospitalID", "WeekDays");
			ViewData["PatientID"] = new SelectList(_context.Patients, "PersonID", "CNIC");
			ViewData["StatusID"] = new SelectList(_context.Lookups, "LookupID", "Category");
			return View();
		}

		// POST: Appointments/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("AppointmentID,PatientID,DoctorID,SelectedDoctorHospitalID,StatusID,TimeCreated,TimeSlot,Prescriptions,TestSuggested")] Appointment appointment)
		{
			if (ModelState.IsValid)
			{
				_context.Add(appointment);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["DoctorID"] = new SelectList(_context.Doctors, "PersonID", "CNIC", appointment.DoctorID);
			ViewData["SelectedDoctorHospitalID"] = new SelectList(_context.DoctorHospitals, "DoctorHospitalID", "WeekDays", appointment.SelectedDoctorHospitalID);
			ViewData["PatientID"] = new SelectList(_context.Patients, "PersonID", "CNIC", appointment.PatientID);
			ViewData["StatusID"] = new SelectList(_context.Lookups, "LookupID", "Category", appointment.StatusID);
			return View(appointment);
		}

		// GET: Appointments/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var appointment = await _context.Appointments.FindAsync(id);
			if (appointment == null)
			{
				return NotFound();
			}
			ViewData["DoctorID"] = new SelectList(_context.Doctors, "PersonID", "CNIC", appointment.DoctorID);
			ViewData["SelectedDoctorHospitalID"] = new SelectList(_context.DoctorHospitals, "DoctorHospitalID", "WeekDays", appointment.SelectedDoctorHospitalID);
			ViewData["PatientID"] = new SelectList(_context.Patients, "PersonID", "CNIC", appointment.PatientID);
			ViewData["StatusID"] = new SelectList(_context.Lookups, "LookupID", "Category", appointment.StatusID);
			return View(appointment);
		}

		// POST: Appointments/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("AppointmentID,PatientID,DoctorID,SelectedDoctorHospitalID,StatusID,TimeCreated,TimeSlot,Prescriptions,TestSuggested")] Appointment appointment)
		{
			if (id != appointment.AppointmentID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(appointment);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!AppointmentExists(appointment.AppointmentID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["DoctorID"] = new SelectList(_context.Doctors, "PersonID", "CNIC", appointment.DoctorID);
			ViewData["SelectedDoctorHospitalID"] = new SelectList(_context.DoctorHospitals, "DoctorHospitalID", "WeekDays", appointment.SelectedDoctorHospitalID);
			ViewData["PatientID"] = new SelectList(_context.Patients, "PersonID", "CNIC", appointment.PatientID);
			ViewData["StatusID"] = new SelectList(_context.Lookups, "LookupID", "Category", appointment.StatusID);
			return View(appointment);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ScheduleAppointment(int doctorId, int hospitalId, DateTime selectedTimeSlot)
		{
			int patientId = int.Parse(Request.Cookies["PersonID"]!);

			var confirmedStatusId = await _context.Lookups
				.Where(l => l.Category == "Status" && l.Value == "Confirmed")
				.Select(l => l.LookupID)
				.FirstOrDefaultAsync();

			if (confirmedStatusId == 0)
			{
				return NotFound("Status IDs for 'Confirmed' not found");
			}

			// Get the associated DoctorHospital record to check capacity and time slots
			var doctorHospital = await _context.DoctorHospitals
				.Include(dh => dh.Appointments)
				.FirstOrDefaultAsync(dh => dh.DoctorID == doctorId && dh.HospitalID == hospitalId);

			if (doctorHospital == null)
			{
				return NotFound("Doctor or Hospital not found");
			}

			// Split and parse the WeekDays range (e.g., "Mon-Fri")
			var daysOfWeek = new Dictionary<string, DayOfWeek>
			{
				{ "Mon", DayOfWeek.Monday },
				{ "Tue", DayOfWeek.Tuesday },
				{ "Wed", DayOfWeek.Wednesday },
				{ "Thu", DayOfWeek.Thursday },
				{ "Fri", DayOfWeek.Friday },
				{ "Sat", DayOfWeek.Saturday },
				{ "Sun", DayOfWeek.Sunday }
			};

			var weekdayParts = doctorHospital.WeekDays.Split('-');
			if (weekdayParts.Length == 2 &&
				daysOfWeek.TryGetValue(weekdayParts[0], out var startDay) &&
				daysOfWeek.TryGetValue(weekdayParts[1], out var endDay))
			{
				// Check if the selected day is within the range
				var selectedDay = selectedTimeSlot.DayOfWeek;
				if (startDay <= endDay)
				{
					if (selectedDay < startDay || selectedDay > endDay)
					{
						return BadRequest("The selected day is not within the doctor's available operating days.");
					}
				}
				else // Handle cases like "Fri-Mon"
				{
					if (selectedDay > endDay && selectedDay < startDay)
					{
						return BadRequest("The selected day is not within the doctor's available operating days.");
					}
				}
			}
			else
			{
				return BadRequest("Invalid weekday format in the database.");
			}

			// Validate that the selected time is within the doctor's available time range
			if (selectedTimeSlot.TimeOfDay < doctorHospital.TimeStart || selectedTimeSlot.TimeOfDay > doctorHospital.TimeEnd)
			{
				return BadRequest("The selected time slot is outside the doctor's available time range.");
			}

			// Check if the capacity for confirmed appointments for the selected day is reached
			var selectedDate = selectedTimeSlot.Date;
			int confirmedAppointmentsCount = doctorHospital.Appointments
				.Count(a => a.TimeSlot.Date == selectedDate && a.StatusID == confirmedStatusId);

			if (confirmedAppointmentsCount >= doctorHospital.Capacity)
			{
				return BadRequest("The doctor's capacity for confirmed appointments on the selected day is full.");
			}

			// Create a new appointment with pending status
			var newAppointment = new Appointment
			{
				DoctorID = doctorId,
				SelectedDoctorHospitalID = doctorHospital.DoctorHospitalID,
				TimeSlot = selectedTimeSlot,
				TimeCreated = DateTime.Now,
				StatusID = confirmedStatusId,
				PatientID = patientId
			};

			// Add the new appointment to the context
			_context.Appointments.Add(newAppointment);
			await _context.SaveChangesAsync();

			// Redirect to a confirmation or details page after successful creation
			return RedirectToAction("Details", new { id = newAppointment.AppointmentID });
		}



		// GET: Appointments/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var appointment = await _context.Appointments
				.Include(a => a.Doctor)
				.Include(a => a.DoctorHospital)
				.Include(a => a.Patient)
				.Include(a => a.Status)
				.FirstOrDefaultAsync(m => m.AppointmentID == id);
			if (appointment == null)
			{
				return NotFound();
			}

			return View(appointment);
		}

		// POST: Appointments/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var appointment = await _context.Appointments.FindAsync(id);
			if (appointment != null)
			{
				_context.Appointments.Remove(appointment);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool AppointmentExists(int id)
		{
			return _context.Appointments.Any(e => e.AppointmentID == id);
		}
	}
}
