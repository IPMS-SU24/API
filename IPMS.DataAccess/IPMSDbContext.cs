using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace IPMS.DataAccess
{
    public class IPMSDbContext : IdentityDbContext<IPMSUser, IdentityRole<Guid>, Guid>
    {
        public IPMSDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AccountComponent>(entity =>
            {
                entity
                    .ToTable("AccountComponent")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Component)
                    .WithMany(p => p.Lecturers)
                    .HasForeignKey("ComponentId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.BorrowedComponents)
                    .HasForeignKey("LecturerId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Assessment>(entity =>
            {
                entity
                    .ToTable("Assessment")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Syllabus)
                    .WithMany(p => p.Assessments)
                    .HasForeignKey("SyllabusId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ClassMember>(entity =>
            {
                entity
                    .ToTable("ClassMember")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Student)
                    .WithMany(p => p.ClassMembers)
                    .HasForeignKey("StudentId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.ClassMembers)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ClassTopic>(entity =>
            {
                entity
                    .ToTable("ClassTopic")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.Topics)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Topic)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("TopicId")
                    .OnDelete(DeleteBehavior.SetNull);

            });

            modelBuilder.Entity<Committee>(entity =>
            {
                entity
                    .ToTable("Committee")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.Committees)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Committees)
                    .HasForeignKey("LecturerId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity
                    .ToTable("Favorite")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey("LecturerId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<IoTComponent>(entity =>
            {
                entity
                    .ToTable("IoTComponent")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);
            });

            modelBuilder.Entity<IPMSClass>(entity =>
            {
                entity
                    .ToTable("IPMSClass")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("LecturerId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<IPMSUser>(entity =>
            {
                entity
                    .ToTable("IPMSUser")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);
            });

            modelBuilder.Entity<LecturerGrade>(entity =>
            {
                entity
                    .ToTable("LecturerGrade")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Committee)
                    .WithMany(p => p.Grades)
                    .HasForeignKey("CommitteeId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MemberHistory>(entity =>
            {
                entity
                    .ToTable("MemberHistory")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

            });

            //hoi lu

            modelBuilder.Entity<MemberProject>(entity =>
            {
                entity
                    .ToTable("MemberProject")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.MemberProjects)
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Student)
                    .WithMany(p => p.MemberProjects)
                    .HasForeignKey("StudentId")
                    .OnDelete(DeleteBehavior.SetNull);


            });

            //con class topic
            modelBuilder.Entity<Project>(entity =>
            {
                entity
                    .ToTable("Project")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.OwnerLecturer)
                    .WithMany(p => p.OwnProjects)
                    .HasForeignKey("OwnerLecturerId")
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne<ClassTopic>(e => e.Topic)
                    .WithOne(e => e.Project)
                    .HasForeignKey<ClassTopic>(e => e.ProjectId);
            });

            modelBuilder.Entity<ProjectComponent>(entity =>
            {
                entity
                    .ToTable("ProjectComponent")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Component)
                    .WithMany(p => p.Projects)
                    .HasForeignKey("ComponentId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Components)
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProjectSubmission>(entity =>
            {
                entity
                    .ToTable("ProjectSubmission")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.SubmissionModule)
                    .WithMany(p => p.ProjectSubmissions)
                    .HasForeignKey("SubmissionModuleId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity
                    .ToTable("Report")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Reporter)
                    .WithMany(p => p.Reports)
                    .HasForeignKey("ReporterId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Semester>(entity =>
            {
                entity
                    .ToTable("Semester")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Syllabus)
                    .WithMany(p => p.Semesters)
                    .HasForeignKey("SyllabusId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SubmissionModule>(entity =>
            {
                entity
                    .ToTable("SubmissionModule")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Assessment)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey("AssessmentId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.SubmissionModules)
                    .HasForeignKey("LectureId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Syllabus>(entity =>
            {
                entity
                    .ToTable("Syllabus")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity
                    .ToTable("Topic")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Owner)
                    .WithMany(p => p.OwnTopics)
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TopicComponent>(entity =>
            {
                entity
                    .ToTable("TopicComponent")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Component)
                    .WithMany(p => p.Topics)
                    .HasForeignKey("ComponentId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Topic)
                    .WithMany(p => p.Components)
                    .HasForeignKey("TopicId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TopicFavorite>(entity =>
            {
                entity
                    .ToTable("TopicFavorite")
                    .HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityColumn(1, 1);

                entity.HasOne(e => e.Topic)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey("TopicId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Favorite)
                    .WithMany(p => p.Topics)
                    .HasForeignKey("FavoriteId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

        }
    }
}
