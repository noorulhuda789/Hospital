using Microsoft.EntityFrameworkCore;
using Health_Hub.Models.Domain;

namespace Health_Hub.Models
{
    public class HealthHubDbContext : DbContext
    {
        public HealthHubDbContext(DbContextOptions<HealthHubDbContext> options) : base(options) 
        { 
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorHospital> DoctorHospitals { get; set; }
        public DbSet<MedicalReport> MedicalReports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Lookup> Lookups { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Chat> Chats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring Person Entity
            modelBuilder.Entity<Person>()
                .ToTable("People")
                .HasKey(p => p.PersonID);

            modelBuilder.Entity<Person>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Person>()
                .Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Person>()
                .HasIndex(p => p.Email)
                .IsUnique(); // Enforce unique constraint on Email

            modelBuilder.Entity<Person>()
                .Property(p => p.CNIC)
                .HasColumnType("bigint");

            modelBuilder.Entity<Person>()
                .HasIndex(p => p.CNIC)
                .IsUnique(); // Enforce unique constraint on CNIC

            modelBuilder.Entity<Person>()
                .Property(p => p.PhoneNumber)
                .HasMaxLength(15);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Role)
                .WithMany(l => l.People)
                .HasForeignKey(p => p.RoleID);



            // Configuring Patient Entity (Inheriting Person)
            modelBuilder.Entity<Patient>()
                .ToTable("Patients");

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Chats)
                .WithOne(c => c.Patient)
                .HasForeignKey(c => c.PatientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Address)
                .HasMaxLength(256);


            // Configuring Doctor Entity (Inheriting Person)
            modelBuilder.Entity<Doctor>()
                .ToTable("Doctors");

            modelBuilder.Entity<Doctor>()
                .Property(d => d.VerificationStatus)
                .HasDefaultValue(false);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Rating)
                .HasDefaultValue(0.0f);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Specialization)
                .WithMany(l => l.Doctors)
                .HasForeignKey(d => d.SpecializationID);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.SelectedDoctorHospitalID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Chats)
                .WithOne(c => c.Doctor)
                .HasForeignKey(c => c.DoctorID);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.DoctorHospitals)
                .WithOne(dh => dh.Doctor)
                .HasForeignKey(dh => dh.DoctorID);
            // Define the relationship between Doctor and Person
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Person)
                .WithOne(p => p.Doctor) // Optional: if Person can have only one related Doctor
                .HasForeignKey<Doctor>(d => d.PersonID);


            // Configuring Hospital Entity
            modelBuilder.Entity<Hospital>()
                .ToTable("Hospitals")
                .HasKey(h => h.HospitalID);

            modelBuilder.Entity<Hospital>()
                .Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Hospital>()
                .Property(h => h.Address)
                .HasMaxLength(300);

            modelBuilder.Entity<Hospital>()
                .Property(h => h.City)
                .HasMaxLength(100);

            modelBuilder.Entity<Hospital>()
                .HasMany(h => h.DoctorHospitals)
                .WithOne(dh => dh.Hospital)
                .HasForeignKey(dh => dh.HospitalID);


            // Configuring DoctorHospital Entity (Many-to-Many)
            modelBuilder.Entity<DoctorHospital>()
                .ToTable("DoctorHospitals")
                .HasKey(dh => dh.DoctorHospitalID);

            modelBuilder.Entity<DoctorHospital>()
                .Property(dh => dh.TimeStart)
                .IsRequired();

            modelBuilder.Entity<DoctorHospital>()
                .Property(dh => dh.TimeEnd)
                .IsRequired();

            modelBuilder.Entity<DoctorHospital>()
                .Property(dh => dh.WeekDays)
                .IsRequired();

            modelBuilder.Entity<DoctorHospital>()
                .Property(dh => dh.Capacity)
                .HasDefaultValue(0);



            // Configuring Appointment Entity
            modelBuilder.Entity<Appointment>()
                .ToTable("Appointments")
                .HasKey(a => a.AppointmentID);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.TimeSlot)
                .IsRequired();

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Prescriptions)
                .HasMaxLength(1000);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.TestSuggested)
                .HasMaxLength(1000);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Status)
                .WithMany(l => l.Appointments)
                .HasForeignKey(a => a.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring Appointment to MedicalReport relationship
            modelBuilder.Entity<Appointment>()
                .HasMany(a => a.MedicalReports)
                .WithOne(mr => mr.Appointment)
                .HasForeignKey(mr => mr.AppointmentID);



            // Configuring Chat Entity
            modelBuilder.Entity<Chat>()
                .ToTable("Chats")
                .HasKey(c => c.ChatID);

            modelBuilder.Entity<Chat>()
                .Property(c => c.Message)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Status) 
                .WithMany() // No collection needed in Lookup
                .HasForeignKey(c => c.StatusID)
                .OnDelete(DeleteBehavior.Restrict);


            // Configuring MedicalReport Entity
            modelBuilder.Entity<MedicalReport>()
                .ToTable("MedicalReports")
                .HasKey(mr => mr.ReportID);

            modelBuilder.Entity<MedicalReport>()
                .Property(mr => mr.ReportDescription)
                .HasMaxLength(500);

            modelBuilder.Entity<MedicalReport>()
                .Property(mr => mr.ReportDocument)
                .IsRequired();


            // Configuring Notification Entity
            modelBuilder.Entity<Notification>()
                .ToTable("Notifications")
                .HasKey(n => n.NotificationID);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(p => p.Notifications)
                .HasForeignKey(n => n.ReceiverID);



            // Configuring Lookup Entity
            modelBuilder.Entity<Lookup>()
                .ToTable("Lookups")
                .HasKey(l => l.LookupID);

            modelBuilder.Entity<Lookup>()
                .Property(l => l.Category)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Lookup>()
                .Property(l => l.Value)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
