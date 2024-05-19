﻿using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection.Emit;

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

        public IPMSDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());
        private string GetConnectionString()
        {
            
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            return configuration["ConnectionStrings:IPMS"];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<Assessment>(entity =>
            {
                entity
                    .ToTable("Assessment")
                    .HasKey(e => e.Id);

                entity.HasOne(e => e.Syllabus)
                    .WithMany(p => p.Assessments)
                    .HasForeignKey("SyllabusId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity
                    .ToTable("Student")
                    .HasKey(e => e.Id);

             
          

                entity.HasOne(e => e.Information)
                    .WithMany(p => p.Students)
                    .HasForeignKey("InformationId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Class)
                    .WithMany(p => p.Students)
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Students)
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ClassTopic>(entity =>
            {
                entity
                    .ToTable("ClassTopic")
                    .HasKey(e => e.Id);

             
          

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

             
          
            });

            modelBuilder.Entity<IPMSClass>(entity =>
            {
                entity
                    .ToTable("IPMSClass")
                    .HasKey(e => e.Id);    

                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Classes)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<IPMSUser>(entity =>
            {
               /* entity
                    .ToTable("IPMSUser")
                    .HasKey(e => e.Id);*/



            });

            modelBuilder.Entity<LecturerGrade>(entity =>
            {
                entity
                    .ToTable("LecturerGrade")
                    .HasKey(e => e.Id);

             
          

                entity.HasOne(e => e.Committee)
                    .WithMany(p => p.Grades)
                    .HasForeignKey("CommitteeId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Submission)
                    .WithMany(p => p.Grades)
                    .HasForeignKey("SubmissionId")
                    .OnDelete(DeleteBehavior.SetNull);
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

             
          

                entity.HasOne(e => e.Owner)
                    .WithMany(p => p.OwnProjects)
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.SetNull);
                
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
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProjectSubmission>(entity =>
            {
                entity
                    .ToTable("ProjectSubmission")
                    .HasKey(e => e.Id);

             
          

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

             
          

                entity.HasOne(e => e.Assessment)
                    .WithMany(p => p.Modules)
                    .HasForeignKey("AssessmentId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Semester)
                    .WithMany(p => p.Modules)
                    .HasForeignKey("SemesterId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Lecturer)
                    .WithMany(p => p.Modules)
                    .HasForeignKey("LectureId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Syllabus>(entity =>
            {
                entity
                    .ToTable("Syllabus")
                    .HasKey(e => e.Id);

             
          

            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity
                    .ToTable("Topic")
                    .HasKey(e => e.Id);

             
          

                entity.HasOne(e => e.Owner)
                    .WithMany(p => p.OwnTopics)
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TopicFavorite>(entity =>
            {
                entity
                    .ToTable("TopicFavorite")
                    .HasKey(e => e.Id);

             
          

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
