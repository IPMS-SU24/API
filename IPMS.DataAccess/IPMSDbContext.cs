using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Reflection.Emit;

namespace IPMS.DataAccess
{
    public class IPMSDbContext : DbContext
    {
        public virtual DbSet<Assessment> Assessments { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<ClassTopic> ClassTopics { get; set; }
        public virtual DbSet<Committee> Committees { get; set; }
        public virtual DbSet<Favorite> Favorites { get; set; }
        public virtual DbSet<IoTComponent> IoTComponents { get; set; }
        public virtual DbSet<IPMSClass> IPMSClasses { get; set; }
        public virtual DbSet<IPMSUser> IPMSUsers { get; set; }
        public virtual DbSet<LecturerGrade> LecturerGrades { get; set; }
        public virtual DbSet<MemberHistory> MemberHistories { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectSubmission> ProjectSubmissions { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Semester> Semesters { get; set; }
        public virtual DbSet<SubmissionModule> SubmissionModules { get; set; }
        public virtual DbSet<Syllabus> Syllabuses { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<ComponentsMaster> ComponentsMasters { get; set; }
        public virtual DbSet<TopicFavorite> TopicFavorites { get; set; }
        public virtual DbSet<ReportType> ReportTypes { get; set; }

        public IPMSDbContext(DbContextOptions<IPMSDbContext> options) : base(options)
        {
        }
        public IPMSDbContext()
        {
            
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(GetConnectionString());
        private string GetConnectionString()
        {

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            return configuration.GetConnectionString("IPMS");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Assessment>(entity =>
            {
                entity
                    .ToTable("Assessment")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(e => e.Syllabus)
                    .WithMany(p => p.Assessments)
                    .HasForeignKey("SyllabusId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity
                    .ToTable("Student")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Information)
                    .WithMany(p => p.Students)
                    .HasForeignKey("InformationId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.Students)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Students)
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClassTopic>(entity =>
            {
                entity
                    .ToTable("ClassTopic")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.Topics)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Topic)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("TopicId")
                    .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<Committee>(entity =>
            {
                entity
                    .ToTable("Committee")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.Committees)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Committees)
                    .HasForeignKey("LecturerId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity
                    .ToTable("Favorite")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey("LecturerId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<IoTComponent>(entity =>
            {
                entity
                    .ToTable("IoTComponent")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<IPMSClass>(entity =>
            {
                entity
                    .ToTable("IPMSClass")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

         /*   modelBuilder.Entity<IPMSUser>(entity =>
            {

            });*/

            modelBuilder.Entity<LecturerGrade>(entity =>
            {
                entity
                    .ToTable("LecturerGrade")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Committee)
                    .WithMany(p => p.Grades)
                    .HasForeignKey("CommitteeId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Submission)
                    .WithMany(p => p.Grades)
                    .HasForeignKey("SubmissionId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MemberHistory>(entity =>
            {
                entity
                    .ToTable("MemberHistory")
                    .HasKey(e => e.Id);

            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity
                    .ToTable("Project")
                    .HasKey(e => e.Id);

                entity.Property(e => e.GroupName).HasMaxLength(50);

                entity.HasOne(e => e.Owner)
                    .WithMany(p => p.OwnProjects)
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<ClassTopic>(e => e.Topic)
                    .WithOne(e => e.Project)
                    .HasForeignKey<ClassTopic>(e => e.ProjectId);
            });

            modelBuilder.Entity<ComponentsMaster>(entity =>
            {
                entity
                    .ToTable("ComponentsMaster")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Component)
                    .WithMany(p => p.ComponentsMasters)
                    .HasForeignKey("ComponentId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProjectSubmission>(entity =>
            {
                entity
                    .ToTable("ProjectSubmission")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SubmissionModule)
                    .WithMany(p => p.ProjectSubmissions)
                    .HasForeignKey("SubmissionModuleId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity
                    .ToTable("Report")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Reporter)
                    .WithMany(p => p.Reports)
                    .HasForeignKey("ReporterId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ReportType)
                    .WithMany(p => p.Reports)
                    .HasForeignKey("ReportTypeId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ReportType>(entity =>
            {
                entity
                    .ToTable("ReportType")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(50);
            });
            modelBuilder.Entity<Semester>(entity =>
            {
                entity
                    .ToTable("Semester")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.HasOne(e => e.Syllabus)
                    .WithMany(p => p.Semesters)
                    .HasForeignKey("SyllabusId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SubmissionModule>(entity =>
            {
                entity
                    .ToTable("SubmissionModule")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(e => e.Assessment)
                    .WithMany(p => p.Modules)
                    .HasForeignKey("AssessmentId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Modules)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Modules)
                    .HasForeignKey("LectureId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Syllabus>(entity =>
            {
                entity
                    .ToTable("Syllabus")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ShortName).HasMaxLength(50);
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity
                    .ToTable("Topic")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description).HasMaxLength(10000);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.HasOne(e => e.Owner)
                    .WithMany(p => p.OwnTopics)
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TopicFavorite>(entity =>
            {
                entity
                    .ToTable("TopicFavorite")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Topic)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey("TopicId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Favorite)
                    .WithMany(p => p.Topics)
                    .HasForeignKey("FavoriteId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //AspNetUser Handle
            modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("RoleClaims");
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.ToTable("AccountRole");
            });
            modelBuilder.Ignore<IdentityUserLogin<Guid>>(); //Ignore UserLogin

            modelBuilder.Entity<IdentityUser<Guid>>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("Account");
                // ignore
                entity.Ignore(u => u.NormalizedUserName);
                entity.Ignore(u => u.NormalizedEmail);
                entity.Ignore(u => u.TwoFactorEnabled);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.LockoutEnd);

                // A concurrency token for use with the optimistic concurrency checking
                entity.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

                // Limit the size of columns to use efficient database types
                entity.Property(u => u.UserName).HasMaxLength(256);
                entity.Property(u => u.Email).HasMaxLength(256);
                // The relationships between User and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each User can have many UserClaims
                entity.HasMany<IdentityUserClaim<Guid>>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();

                // Each User can have many UserTokens
                entity.HasMany<IdentityUserToken<Guid>>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();

                // Each User can have many entries in the UserRole join table
                entity.HasMany<IdentityUserRole<Guid>>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            });

            modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("AccountClaims");
            });


            modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.ToTable("AccountTokens");

                // Limit the size of the composite key columns due to common DB restrictions
                entity.Property(t => t.LoginProvider).HasMaxLength(10000);
                entity.Property(t => t.Name).HasMaxLength(10000);

            });

            modelBuilder.Entity<IdentityRole<Guid>>(entity =>
            {
                entity.HasKey(r => r.Id);
                // Index for "normalized" role name to allow efficient lookups
                entity.Ignore(r => r.NormalizedName);

                entity.ToTable("Role");

                // A concurrency token for use with the optimistic concurrency checking
                entity.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

                // Limit the size of columns to use efficient database types
                entity.Property(u => u.Name).HasMaxLength(256);
                entity.Ignore(u => u.NormalizedName);

                // The relationships between Role and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each Role can have many entries in the UserRole join table
                entity.HasMany<IdentityUserRole<Guid>>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();

                // Each Role can have many associated RoleClaims
                entity.HasMany<IdentityRoleClaim<Guid>>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
            });

          

        }
    }
}
