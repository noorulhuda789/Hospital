using Health_Hub.Models;
using Health_Hub.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Health_Hub.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
namespace Health_Hub.Controllers
{
	public class SignUpDoctorController : Controller
	{
		private readonly HealthHubDbContext _context;
		public SignUpDoctorController(HealthHubDbContext context)
		{
			_context = context;
		}
		public IActionResult SignUpDoctor()
		{
			//// Assuming you have a DbContext called _context
			//var roles = _context.Lookups
			//			.Where(l => l.Category == "Role")
			//			.Select(l => new SelectListItem
			//			{
			//				Value = l.LookupID.ToString(),  
			//				Text = l.Value                  // Display the role name (Patient, Doctor)
			//			}).ToList();

			//ViewBag.RoleID = roles;

			return View();
		}
        [HttpPost]
        public async Task<IActionResult> SignUpDoctor(Person person)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (ModelState.IsValid)
                {
                    // Raw SQL query to insert into People and get PersonID
                    var insertPersonQuery = @"
                INSERT INTO People (Name, Email, CNIC, Password, PhoneNumber, RoleID) 
                VALUES (@Name, @Email, @CNIC, @Password, @PhoneNumber, @RoleID);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";  // Capture the generated PersonID

                    int personID;
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = insertPersonQuery;
                        command.Transaction = _context.Database.CurrentTransaction.GetDbTransaction();

                        command.Parameters.Add(new SqlParameter("@Name", person.Name));
                        command.Parameters.Add(new SqlParameter("@Email", person.Email));
                        command.Parameters.Add(new SqlParameter("@CNIC", person.CNIC));
                        command.Parameters.Add(new SqlParameter("@Password", person.Password));
                        command.Parameters.Add(new SqlParameter("@PhoneNumber", person.PhoneNumber));
                        command.Parameters.Add(new SqlParameter("@RoleID", person.RoleID));

                        await _context.Database.OpenConnectionAsync();

                        personID = (int)await command.ExecuteScalarAsync();
                    }

                    // Insert into Doctor or Patient table based on RoleID
                    //if (person.RoleID == 3) // Patient
                    //{
                    //    var insertPatientQuery = "INSERT INTO Patients (PersonID) VALUES (@PersonID);";
                    //    await _context.Database.ExecuteSqlRawAsync(insertPatientQuery, new SqlParameter("@PersonID", personID));
                    //}
                    if (person.RoleID == 4) // Doctor
                    {
                        var insertDoctorQuery = "INSERT INTO Doctors (PersonID) VALUES (@PersonID);";
                        await _context.Database.ExecuteSqlRawAsync(insertDoctorQuery, new SqlParameter("@PersonID", personID));
                    }

                    await transaction.CommitAsync();

                    //if (person.RoleID == 3)
                    //{
                    //    return RedirectToAction("IndexForPatient", "Home");
                    //}
                    if (person.RoleID == 4)
                    {
                        return RedirectToAction("IndexForDoctor", "Home");
                    }
                }

                return View(person);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", ex.Message);
                return View(person);
            }
        }


    }

}
