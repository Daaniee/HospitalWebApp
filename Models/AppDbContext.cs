using Microsoft.EntityFrameworkCore;

namespace hospitalwebapp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<AvailableDoctor> AvailableDoctors { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Enum as string
            modelBuilder
                .Entity<MedicalRecord>()
                .Property(m => m.Status)
                .HasConversion<string>();

            // 🔐 Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Receptionist" },
                new Role { Id = 2, Name = "Nurse" },
                new Role { Id = 3, Name = "Doctor" },
                new Role { Id = 4, Name = "Admin" }
            );
            // 🔐 Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Action = "RegisterPatient" },
                new Permission { Id = 2, Action = "EditPatientInfo" },
                new Permission { Id = 3, Action = "ViewPatients" },
                new Permission { Id = 4, Action = "ScheduleAppointment" },
                new Permission { Id = 5, Action = "AssignDoctor" },
                new Permission { Id = 6, Action = "EditMedicalRecord" },
                new Permission { Id = 7, Action = "ViewMedicalRecord" },
                new Permission { Id = 8, Action = "Diagnose" },
                new Permission { Id = 9, Action = "PrescribeMedication" }
            );

            //new permissions
            modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 10, Action = "ManageRoles" },
            new Permission { Id = 11, Action = "ManagePermissions" },
            new Permission { Id = 12, Action = "ViewAuditLogs" },
            new Permission { Id = 13, Action = "DeletePatient" },
            new Permission { Id = 14, Action = "UpdateStaffInfo" }
        );

            // 🔗 Configure RolePermission many-to-many
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            
            
modelBuilder.Entity<RolePermission>()
    .HasOne(rp => rp.Role)
    .WithMany(r => r.RolePermissions)
    .HasForeignKey(rp => rp.RoleId)
    .OnDelete(DeleteBehavior.NoAction); // 👈 prevent cascade

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 prevent cascade
            // 🔐 Seed RolePermission   s
            modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);

    

            modelBuilder.Entity<RolePermission>().HasData(
                // Receptionist
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 },

                // Nurse
                new RolePermission { RoleId = 2, PermissionId = 3 },
                new RolePermission { RoleId = 2, PermissionId = 5 },
                new RolePermission { RoleId = 2, PermissionId = 6 },
                new RolePermission { RoleId = 2, PermissionId = 7 },

                // Doctor
                new RolePermission { RoleId = 3, PermissionId = 3 },
                new RolePermission { RoleId = 3, PermissionId = 6 },
                new RolePermission { RoleId = 3, PermissionId = 7 },
                new RolePermission { RoleId = 3, PermissionId = 8 },
                new RolePermission { RoleId = 3, PermissionId = 9 },

                // Admin
                new RolePermission { RoleId = 4, PermissionId = 10 },
                new RolePermission { RoleId = 4, PermissionId = 11 },
                new RolePermission { RoleId = 4, PermissionId = 12 },
                new RolePermission { RoleId = 4, PermissionId = 13 },
                new RolePermission { RoleId = 4, PermissionId = 14 }
            );
        }
    }
}