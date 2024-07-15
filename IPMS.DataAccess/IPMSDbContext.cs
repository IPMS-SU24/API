using IPMS.DataAccess.Common.Extensions;
using IPMS.DataAccess.Common.Models;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace IPMS.DataAccess
{
    public class IPMSDbContext : IdentityDbContext<IPMSUser, IdentityRole<Guid>, Guid>
    {
        public virtual DbSet<Assessment> Assessments { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<ClassTopic> ClassTopics { get; set; }
        public virtual DbSet<Committee> Committees { get; set; }
        public virtual DbSet<Favorite> Favorites { get; set; }
        public virtual DbSet<IoTComponent> IoTComponents { get; set; }
        public virtual DbSet<IPMSClass> IPMSClasses { get; set; }
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
        private string _connectionString = string.Empty;

        public IPMSDbContext(DbContextOptions<IPMSDbContext> options) : base(options)
        {
        }
        public IPMSDbContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
                optionsBuilder.UseNpgsql(configuration.GetConnectionString("IPMS"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            //Rename IdentityTables
            modelBuilder.Entity<IPMSUser>(entity =>
            {
                entity.ToTable("Account");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.LastModified).HasDefaultValueSql("now()");
                entity.Property(e => e.PhoneNumber).IsRequired(false);
                entity.Ignore(c => c.AccessFailedCount);
                entity.Ignore(c => c.LockoutEnabled);
                entity.Ignore(c => c.LockoutEnd);
                entity.Ignore(c => c.TwoFactorEnabled);
            });
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Role");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity
                    .ToTable("Assessment")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Percentage).HasPrecision(3, 0);

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

                entity.Property(e => e.ContributePercentage).HasPrecision(3, 0);

                entity.Property(e => e.FinalGrade).HasPrecision(4, 2);
                entity.HasIndex(e => new {e.ClassId,e.InformationId }).IsUnique();
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

                entity.Property(e => e.Percentage).HasPrecision(3, 0);

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

                entity.Property(e => e.Description);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<IPMSClass>(entity =>
            {
                entity
                    .ToTable("IPMSClass")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Name);

                entity.Property(e => e.ShortName).HasMaxLength(50);
                entity.HasIndex(e => e.ShortName).IsUnique();
                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LecturerGrade>(entity =>
            {
                entity
                    .ToTable("LecturerGrade")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Grade).HasPrecision(4, 2);


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
                entity.Ignore(x => x.FinalStatus);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity
                    .ToTable("Project")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Grade).HasPrecision(4, 2);

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
                entity.Property(e=>e.Status).IsRequired(false);
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

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.FinalGrade).HasPrecision(4, 2);

                

                entity.HasOne(e => e.Submitter)
                    .WithMany(p => p.ProjectSubmissions)
                    .HasForeignKey("SubmitterId")
                    .OnDelete(DeleteBehavior.Cascade);

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
                entity.Property(e => e.ResponseContent).IsRequired(false);
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
                entity.HasIndex(e=>e.ShortName).IsUnique();
                entity.Property(e => e.Description);

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

                entity.Property(e => e.Percentage).HasPrecision(3, 0);

                entity.Property(e => e.Description);

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
                entity.HasIndex(e => e.Id).IsUnique();
                entity.Property(e => e.Description);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ShortName).HasMaxLength(50);
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity
                    .ToTable("Topic")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Description);
                entity.Property(e => e.Detail);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.HasOne(e => e.Owner)
                    .WithMany(p => p.OwnTopics)
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Suggester)
                   .WithMany(p => p.SuggestedTopic)
                   .HasForeignKey("SuggesterId")
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
            modelBuilder.Entity<ClassModuleDeadline>(entity =>
            {
                entity
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.SubmissionModule)
                    .WithMany(p => p.ClassModuleDeadlines)
                    .HasForeignKey(x=>x.SubmissionModuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.ClassModuleDeadlines)
                    .HasForeignKey(x=>x.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes().Where(x => !x.IsOwned()))
            {
                // check if current entity type is child of BaseModel
                if (typeof(IBaseModel).IsAssignableFrom(mutableEntityType.ClrType))
                {
                    mutableEntityType.ApplySoftDeleteFilter();
                }
            }
        }
    }
}
