﻿// <auto-generated />
using System;
using Health_Hub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Health_Hub.Migrations
{
    [DbContext(typeof(HealthHubDbContext))]
    [Migration("20241019095317_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Health_Hub.Models.Domain.Appointment", b =>
                {
                    b.Property<int>("AppointmentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AppointmentID"));

                    b.Property<int>("DoctorHospitalID")
                        .HasColumnType("int");

                    b.Property<int>("DoctorHospitalID1")
                        .HasColumnType("int");

                    b.Property<int>("DoctorID")
                        .HasColumnType("int");

                    b.Property<int>("PatientID")
                        .HasColumnType("int");

                    b.Property<string>("Prescriptions")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("StatusID")
                        .HasColumnType("int");

                    b.Property<string>("TestSuggested")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<DateTime>("TimeCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TimeSlot")
                        .HasColumnType("datetime2");

                    b.Property<string>("VerificationID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AppointmentID");

                    b.HasIndex("DoctorHospitalID");

                    b.HasIndex("DoctorHospitalID1");

                    b.HasIndex("PatientID");

                    b.HasIndex("StatusID");

                    b.ToTable("Appointments", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Chat", b =>
                {
                    b.Property<int>("ChatID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChatID"));

                    b.Property<int>("DoctorID")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("PatientID")
                        .HasColumnType("int");

                    b.Property<int>("StatusID")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimeSent")
                        .HasColumnType("datetime2");

                    b.HasKey("ChatID");

                    b.HasIndex("DoctorID");

                    b.HasIndex("PatientID");

                    b.HasIndex("StatusID");

                    b.ToTable("Chats", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.DoctorHospital", b =>
                {
                    b.Property<int>("DoctorHospitalID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DoctorHospitalID"));

                    b.Property<TimeSpan?>("BreakEnd")
                        .HasColumnType("time");

                    b.Property<TimeSpan?>("BreakStart")
                        .HasColumnType("time");

                    b.Property<int>("Capacity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("DoctorID")
                        .HasColumnType("int");

                    b.Property<int>("HospitalID")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("TimeEnd")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("TimeStart")
                        .HasColumnType("time");

                    b.Property<string>("WeekDays")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DoctorHospitalID");

                    b.HasIndex("DoctorID");

                    b.HasIndex("HospitalID");

                    b.ToTable("DoctorHospitals", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Hospital", b =>
                {
                    b.Property<int>("HospitalID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("HospitalID"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("HospitalID");

                    b.ToTable("Hospitals", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Lookup", b =>
                {
                    b.Property<int>("LookupID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LookupID"));

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("LookupID");

                    b.ToTable("Lookups", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.MedicalReport", b =>
                {
                    b.Property<int>("ReportID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportID"));

                    b.Property<int>("AppointmentID")
                        .HasColumnType("int");

                    b.Property<int?>("DoctorPersonID")
                        .HasColumnType("int");

                    b.Property<string>("ReportDescription")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("ReportDocument")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReportID");

                    b.HasIndex("AppointmentID");

                    b.HasIndex("DoctorPersonID");

                    b.ToTable("MedicalReports", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Notification", b =>
                {
                    b.Property<int>("NotificationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NotificationID"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("ReceiverID")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimeSent")
                        .HasColumnType("datetime2");

                    b.HasKey("NotificationID");

                    b.HasIndex("ReceiverID");

                    b.ToTable("Notifications", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Person", b =>
                {
                    b.Property<int>("PersonID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PersonID"));

                    b.Property<long>("CNIC")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<int>("RoleID")
                        .HasColumnType("int");

                    b.HasKey("PersonID");

                    b.HasIndex("CNIC")
                        .IsUnique();

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("RoleID");

                    b.ToTable("People", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Doctor", b =>
                {
                    b.HasBaseType("Health_Hub.Models.Domain.Person");

                    b.Property<byte[]>("Degree")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("ProfileImage")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<float>("Rating")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<int>("SpecializationID")
                        .HasColumnType("int");

                    b.Property<bool>("VerificationStatus")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.HasIndex("SpecializationID");

                    b.ToTable("Doctors", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Patient", b =>
                {
                    b.HasBaseType("Health_Hub.Models.Domain.Person");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.ToTable("Patients", (string)null);
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Appointment", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Doctor", "Doctor")
                        .WithMany("Appointments")
                        .HasForeignKey("DoctorHospitalID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.DoctorHospital", "DoctorHospital")
                        .WithMany()
                        .HasForeignKey("DoctorHospitalID1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Patient", "Patient")
                        .WithMany("Appointments")
                        .HasForeignKey("PatientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Lookup", "Status")
                        .WithMany("Appointments")
                        .HasForeignKey("StatusID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Doctor");

                    b.Navigation("DoctorHospital");

                    b.Navigation("Patient");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Chat", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Doctor", "Doctor")
                        .WithMany("Chats")
                        .HasForeignKey("DoctorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Patient", "Patient")
                        .WithMany("Chats")
                        .HasForeignKey("PatientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Lookup", "Status")
                        .WithMany()
                        .HasForeignKey("StatusID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Doctor");

                    b.Navigation("Patient");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.DoctorHospital", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Doctor", "Doctor")
                        .WithMany("DoctorHospitals")
                        .HasForeignKey("DoctorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Hospital", "Hospital")
                        .WithMany("DoctorHospitals")
                        .HasForeignKey("HospitalID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Doctor");

                    b.Navigation("Hospital");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.MedicalReport", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Appointment", "Appointment")
                        .WithMany("MedicalReports")
                        .HasForeignKey("AppointmentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Doctor", null)
                        .WithMany("MedicalReports")
                        .HasForeignKey("DoctorPersonID");

                    b.Navigation("Appointment");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Notification", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Person", "Receiver")
                        .WithMany("Notifications")
                        .HasForeignKey("ReceiverID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Person", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Lookup", "Role")
                        .WithMany("People")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Doctor", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Person", null)
                        .WithOne()
                        .HasForeignKey("Health_Hub.Models.Domain.Doctor", "PersonID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Health_Hub.Models.Domain.Lookup", "Specialization")
                        .WithMany("Doctors")
                        .HasForeignKey("SpecializationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Specialization");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Patient", b =>
                {
                    b.HasOne("Health_Hub.Models.Domain.Person", null)
                        .WithOne()
                        .HasForeignKey("Health_Hub.Models.Domain.Patient", "PersonID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Appointment", b =>
                {
                    b.Navigation("MedicalReports");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Hospital", b =>
                {
                    b.Navigation("DoctorHospitals");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Lookup", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("Doctors");

                    b.Navigation("People");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Person", b =>
                {
                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Doctor", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("Chats");

                    b.Navigation("DoctorHospitals");

                    b.Navigation("MedicalReports");
                });

            modelBuilder.Entity("Health_Hub.Models.Domain.Patient", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("Chats");
                });
#pragma warning restore 612, 618
        }
    }
}
