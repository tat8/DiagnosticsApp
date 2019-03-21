using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class DiagnosticsDBContext : DbContext
    {
        public DiagnosticsDBContext()
        {
        }

        public DiagnosticsDBContext(DbContextOptions<DiagnosticsDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appointment> Appointment { get; set; }
        public virtual DbSet<AppointmentDiagnosis> AppointmentDiagnosis { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<Diagnosis> Diagnosis { get; set; }
        public virtual DbSet<Diagnostics> Diagnostics { get; set; }
        public virtual DbSet<Examination> Examination { get; set; }
        public virtual DbSet<Image> Image { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserPassword> UserPassword { get; set; }

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //        {
        //            if (!optionsBuilder.IsConfigured)
        //            {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //                optionsBuilder.UseSqlServer("Server=LAPTOP123;Database=DiagnosticsDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
        //            }
        //        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasIndex(e => new { e.DoctorId, e.StartTime })
                    .HasName("IX_Appointment")
                    .IsUnique();

                entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");

                entity.Property(e => e.ClientId).HasColumnName("clientId");

                entity.Property(e => e.DoctorId).HasColumnName("doctorId");

                entity.Property(e => e.ExaminationId).HasColumnName("examinationId");

                entity.Property(e => e.Prescription)
                    .IsRequired()
                    .HasColumnName("prescription")
                    .HasMaxLength(200);

                entity.Property(e => e.StartTime)
                    .HasColumnName("startTime")
                    .HasColumnType("datetime");

            });

            modelBuilder.Entity<AppointmentDiagnosis>(entity =>
            {
                entity.HasKey(e => new { e.AppointmentId, e.DiagnosisId });

                entity.HasIndex(e => new { e.AppointmentId, e.DiagnosisId })
                    .HasName("IX_AppointmentDiagnosis")
                    .IsUnique();

                entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");

                entity.Property(e => e.DiagnosisId).HasColumnName("diagnosisId");

            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(e => e.Snils)
                    .HasName("IX_Client")
                    .IsUnique();

                entity.Property(e => e.ClientId).HasColumnName("clientId");

                entity.Property(e => e.BirthDate)
                    .HasColumnName("birthDate")
                    .HasColumnType("date");

                entity.Property(e => e.FatherName)
                    .HasColumnName("fatherName")
                    .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("firstName")
                    .HasMaxLength(50);

                entity.Property(e => e.IsMale).HasColumnName("isMale");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("lastName")
                    .HasMaxLength(50);

                entity.Property(e => e.Passport)
                    .IsRequired()
                    .HasColumnName("passport")
                    .HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(10);

                entity.Property(e => e.Snils)
                    .IsRequired()
                    .HasColumnName("SNILS")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Diagnosis>(entity =>
            {
                entity.HasIndex(e => e.DiagnosisName)
                    .HasName("IX_Diagnosis")
                    .IsUnique();

                entity.Property(e => e.DiagnosisId).HasColumnName("diagnosisId");

                entity.Property(e => e.DiagnosisName)
                    .IsRequired()
                    .HasColumnName("diagnosisName")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Diagnostics>(entity =>
            {
                entity.HasIndex(e => new { e.DoctorId, e.StartTime })
                    .HasName("IX_Diagnostics")
                    .IsUnique();

                entity.Property(e => e.DiagnosticsId).HasColumnName("diagnosticsId");

                entity.Property(e => e.ClientId).HasColumnName("clientId");

                entity.Property(e => e.DoctorId).HasColumnName("doctorId");

                entity.Property(e => e.ExaminationId).HasColumnName("examinationId");

                entity.Property(e => e.StartTime)
                    .HasColumnName("startTime")
                    .HasColumnType("datetime");

            });

            modelBuilder.Entity<Examination>(entity =>
            {
                entity.Property(e => e.ExaminationId)
                    .HasColumnName("examinationId")
                    .ValueGeneratedNever();

                entity.Property(e => e.Breath)
                    .IsRequired()
                    .HasColumnName("breath")
                    .HasMaxLength(50);

                entity.Property(e => e.Complaint)
                    .IsRequired()
                    .HasColumnName("complaint")
                    .HasMaxLength(200);

                entity.Property(e => e.Height).HasColumnName("height");

                entity.Property(e => e.Other)
                    .HasColumnName("other")
                    .HasMaxLength(200);

                entity.Property(e => e.Pressure)
                    .IsRequired()
                    .HasColumnName("pressure")
                    .HasMaxLength(50);

                entity.Property(e => e.Temperature).HasColumnName("temperature");

                entity.Property(e => e.Weight).HasColumnName("weight");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.Property(e => e.ImageId).HasColumnName("imageId");

                entity.Property(e => e.CalcinateBiggest).HasColumnName("calcinateBiggest");

                entity.Property(e => e.CalcinatesCount).HasColumnName("calcinatesCount");

                entity.Property(e => e.CalcinatesPercent).HasColumnName("calcinatesPercent");

                entity.Property(e => e.DiagnosticsId).HasColumnName("diagnosticsId");

                entity.Property(e => e.RefNotParsed)
                    .IsRequired()
                    .HasColumnName("refNotParsed")
                    .HasMaxLength(50);

                entity.Property(e => e.RefParsed)
                    .HasColumnName("refParsed")
                    .HasMaxLength(50);

            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.RoleName)
                    .HasName("IX_Role")
                    .IsUnique();

                entity.Property(e => e.RoleId).HasColumnName("roleId");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasColumnName("roleName")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Inn)
                    .HasName("IX_User")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneNumber)
                    .HasName("IX_UserPhoneNumber")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.FatherName)
                    .HasColumnName("fatherName")
                    .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("firstName")
                    .HasMaxLength(50);

                entity.Property(e => e.Inn)
                    .IsRequired()
                    .HasColumnName("INN")
                    .HasMaxLength(12);

                entity.Property(e => e.PhoneNumber)
                   .IsRequired()
                   .HasColumnName("phoneNumber")
                   .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("lastName")
                    .HasMaxLength(50);

                entity.Property(e => e.RoleId).HasColumnName("roleId");

            });

            modelBuilder.Entity<UserPassword>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("userId");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(50);

            });
        }
    }
}
