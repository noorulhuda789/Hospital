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
using Health_Hub.Models.ViewModels;

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
		public async Task<IActionResult> List(string status = "Confirmed")
		{
			var appointments = _context.Appointments
		   .Include(a => a.Doctor)
		   .Include(a => a.Patient)
		   .Include(a => a.DoctorHospital)
           .ThenInclude(dh => dh.Hospital) // Include the Hospital related to DoctorHospital
           .Include(a => a.Status)
		   .AsQueryable(); // Ensures the entire query stays as IQueryable

			switch (status)
			{
				case "Canceled":
					appointments = appointments.Where(a => a.Status.Value == "Canceled");
                    ViewData["AppointmentCurrentStatus"] = "Canceled";
                    break;
				case "Completed":
					appointments = appointments.Where(a => a.Status.Value == "Completed");
                    ViewData["AppointmentCurrentStatus"] = "Completed";
                    break;
				default: // Default is Approved for upcoming appointments
					appointments = appointments.Where(a => a.Status.Value == "Confirmed");
                    ViewData["AppointmentCurrentStatus"] = "Confirmed";
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


		public async Task<IActionResult> Single(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

            var appointment = await _context.Appointments
                .Include(a => a.Doctor) // Includes doctor details
                .ThenInclude(d => d.Specialization) // Includes specialization details for doctor
                .Include(a => a.Patient) // Includes patient details
                .Include(a => a.DoctorHospital) // Includes the doctor-hospital relation
                    .ThenInclude(dh => dh.Hospital) // Includes hospital details
                .Include(a => a.Status) // Includes status details
                .FirstOrDefaultAsync(a => a.AppointmentID == id);

            if (appointment == null)
			{
				return NotFound();
			}

			// Fetch related medical reports
			var reports = await _context.MedicalReports
				.Where(r => r.AppointmentID == id)
				.ToListAsync();

            ViewData["Layout"] = "_LayoutLogInPatient";
            // Pass data to the view as a tuple
            return View((appointment, reports));
		}



		[HttpPost]
		public async Task<IActionResult> UploadReport(IFormFile ReportDocument, string ReportDescription, int AppointmentID)
		{
			// Fetch the appointment to get the DoctorPersonID
			var appointment = await _context.Appointments
				.FirstOrDefaultAsync(a => a.AppointmentID == AppointmentID);

			if (appointment == null)
			{
				return NotFound("Appointment not found");
			}

			if (ReportDocument != null && ReportDocument.Length > 0)
			{
				// Save the uploaded file to the server
				var filePath = Path.Combine("wwwroot/Images/reports", ReportDocument.FileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await ReportDocument.CopyToAsync(stream);
				}

				// Create a new medical report
				var newReport = new MedicalReport
				{
					ReportDescription = ReportDescription,
					ReportDocument = "/Images/reports/" + ReportDocument.FileName,
					AppointmentID = AppointmentID,
					DoctorPersonID = appointment.DoctorID // Use the DoctorID from the appointment
				};

				// Save the new report to the database
				_context.MedicalReports.Add(newReport);
				await _context.SaveChangesAsync();
			}

			// Redirect to the details page
			return RedirectToAction(nameof(Single), new { id = AppointmentID });
		}

        public async Task<IActionResult> ManageAppointments()
        {
            int doctorId = int.Parse(Request.Cookies["PersonID"]!);

            //int doctorId = 2; // Custom method to fetch logged-in doctor ID

            var appointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Include(a => a.Patient)
                .Include(a => a.Status)
                .ToListAsync();
            ViewData["Layout"] = "_LayoutDoctorLogIn";
            return View(appointments);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var approvedStatusId = await _context.Lookups
                .Where(l => l.Category == "Status" && l.Value == "Confirmed")
                .Select(l => l.LookupID)
                .FirstOrDefaultAsync();

            if (approvedStatusId == 0) return NotFound("Status not found");

            appointment.StatusID = approvedStatusId;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            ViewData["Layout"] = "_LayoutDoctorLogIn";
            return RedirectToAction(nameof(ManageAppointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var cancelledStatusId = await _context.Lookups
                .Where(l => l.Category == "Status" && l.Value == "Canceled")
                .Select(l => l.LookupID)
                .FirstOrDefaultAsync();

            if (cancelledStatusId == 0) return NotFound("Status not found");

            appointment.StatusID = cancelledStatusId;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            ViewData["Layout"] = "_LayoutDoctorLogIn";

            return RedirectToAction(nameof(ManageAppointments));
        }


        public async Task<IActionResult> LoadEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the appointment with all related data
            var appointment = await _context.Appointments
                .Include(a => a.Patient) // Load Patient data
                .Include(a => a.Doctor)  // Load Doctor data
				.ThenInclude(d => d.Specialization)
                .Include(a => a.DoctorHospital) // Load DoctorHospital data
                    .ThenInclude(dh => dh.Hospital) // Load related Hospital data
                .Include(a => a.Status) // Load Status (lookup)
                .FirstOrDefaultAsync(a => a.AppointmentID == id);

            if (appointment == null)
            {
                return NotFound();
            }

            ViewData["Layout"] = "_LayoutDoctorLogIn";

            // Return the populated model to the "Edit" view
            return View("Edit", appointment);
        }




        /*public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentID == id);

            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["Layout"] = "_LayoutDoctorLogIn";
            // Redirect to an edit view to allow the user to input details
            return View("Edit", appointment);
        }*/
        // For the GET method
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentID,TestSuggested,Prescriptions")] Appointment appointment)
        {
            // Fetch the existing appointment from the database
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == appointment.AppointmentID);

            if (existingAppointment == null)
            {
                return NotFound();
            }

            // Manually remove validation errors for non-editable fields
            ModelState.Remove("Doctor");
            ModelState.Remove("Patient");
            ModelState.Remove("DoctorHospital");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only the editable fields
                    existingAppointment.TestSuggested = appointment.TestSuggested;
                    existingAppointment.Prescriptions = appointment.Prescriptions;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(CompletedAppointments));
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
            }

            // Reload related data if validation fails
            ViewData["Layout"] = "_LayoutDoctorLogIn";
            return View(existingAppointment);
        }




        //// For the POST method
        //[HttpPost]
        //public IActionResult EditPost(Appointment appointment)
        //{
        //    // Check if the model state is valid
        //    if (!ModelState.IsValid)
        //    {
        //        TempData["ErrorMessage"] = "Please correct the errors in the form before submitting.";
        //        return View("Edit", appointment);
        //    }

        //    // Fetch the existing appointment from the database
        //    var existingAppointment = _context.Appointments
        //        .Include(a => a.Doctor)
        //        .Include(a => a.DoctorHospital.Hospital)
        //        .FirstOrDefault(a => a.AppointmentID == appointment.AppointmentID);

        //    // Check if the appointment exists
        //    if (existingAppointment != null)
        //    {
        //        // Ensure that DoctorID and StatusID are valid (basic validation)
        //        if (!_context.Doctors.Any(d => d.PersonID == appointment.DoctorID))
        //        {
        //            ModelState.AddModelError("DoctorID", "Invalid Doctor selected.");
        //            return View("Edit", appointment);
        //        }

        //        if (!_context.Lookups.Any(l => l.LookupID == appointment.StatusID))
        //        {
        //            ModelState.AddModelError("StatusID", "Invalid status selected.");
        //            return View("Edit", appointment);
        //        }

        //        // Update the properties that can be modified
        //        existingAppointment.Prescriptions = appointment.Prescriptions;
        //        existingAppointment.TestSuggested = appointment.TestSuggested;
        //        existingAppointment.TimeSlot = appointment.TimeSlot;

        //        try
        //        {
        //            _context.SaveChanges();
        //            TempData["SuccessMessage"] = "Appointment updated successfully!";
        //            return RedirectToAction("CompletedAppointments");
        //        }
        //        catch (Exception ex)
        //        {
        //            //_logger.LogError(ex, "Error updating appointment with ID: {AppointmentID}", appointment.AppointmentID);
        //            TempData["ErrorMessage"] = "An error occurred while updating the appointment: " + ex.Message;
        //            return View("Edit", appointment);
        //        }
        //    }

        //    TempData["ErrorMessage"] = "Appointment not found.";
        //    return View("Edit", appointment);
        //}

        // POST: /Appointments/Reschedule
        [HttpPost]
		public async Task<IActionResult> Reschedule([FromBody] RescheduleViewModel model)
		{
			if (model == null || model.AppointmentID == 0 || model.NewTimeSlot == DateTime.MinValue)
			{
				return BadRequest("Invalid input data.");
			}

			var appointment = await _context.Appointments.FindAsync(model.AppointmentID);
			if (appointment == null)
			{
				return NotFound("Appointment not found.");
			}

			// Update the appointment's time slot
			appointment.TimeSlot = model.NewTimeSlot;

			// Save changes to the database
			await _context.SaveChangesAsync();

			return Ok(new { message = "Appointment rescheduled successfully." });
		}

		// POST: /Appointments/Cancel
		[HttpPost]
		public async Task<IActionResult> Cancel([FromBody] CancelViewModel model)
		{
			if (model == null || model.AppointmentID == 0)
			{
				return BadRequest("Invalid input data.");
			}

			var appointment = await _context.Appointments.FindAsync(model.AppointmentID);
			if (appointment == null)
			{
				return NotFound("Appointment not found.");
			}

			// Update the appointment's status to canceled
			appointment.StatusID = GetStatusId("Canceled");

			// Save changes to the database
			await _context.SaveChangesAsync();

			return Ok(new { message = "Appointment canceled successfully." });
		}

		private int GetStatusId(string statusName)
		{
			var status = _context.Lookups.FirstOrDefault(s => s.Value == statusName);
			return status?.LookupID ?? 0; // Default to 0 if status not found
		}








        public IActionResult CompletedAppointments(string searchCnic)
        {
            // Fetch all completed appointments
            var completedAppointments = _context.Appointments
                .Where(a => a.Status.Value == "Completed")
                .Include(a => a.Status)
                .Include(a => a.Patient)
                .ToList();

            // Filter by CNIC if a search query is provided
            if (!string.IsNullOrWhiteSpace(searchCnic))
            {
                completedAppointments = completedAppointments
                    .Where(a => a.Patient != null && a.Patient.CNIC != null &&
                                a.Patient.CNIC.Contains(searchCnic, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                ViewData["SearchCnic"] = searchCnic; // Pass the search query back to the view
            }

            // Optional: Log or check for null values
            foreach (var appointment in completedAppointments)
            {
                if (appointment.Patient == null)
                {
                    Console.WriteLine($"Appointment ID {appointment.AppointmentID} has a null Patient.");
                }
            }

            ViewData["Layout"] = "_LayoutDoctorLogIn";

            return View(completedAppointments);
        }



    }
}
