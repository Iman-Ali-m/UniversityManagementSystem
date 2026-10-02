using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Data;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
    }

    public DbSet<User> Users=>Set<User>();
    public DbSet<Course> Courses=>Set<Course>();
    public DbSet<Enrollment> Enrollments=>Set<Enrollment>();
    public DbSet<Exam> Exams=>Set<Exam>();
    public DbSet<ExamQuestion> ExamQuestions=>Set<ExamQuestion>();
    public DbSet<ExamOption> ExamOptions=>Set<ExamOption>();
    public DbSet<StudentAnswer> StudentAnswers=>Set<StudentAnswer>();
    public DbSet<Assignment> Assignments=>Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions=>Set<AssignmentSubmission>();
    public DbSet<Grade> Grades=>Set<Grade>();
    public DbSet<StaffCode> StaffCodes=>Set<StaffCode>();
    public DbSet<ProfessorCode> ProfessorCodes=>Set<ProfessorCode>();
    public DbSet<StudentCode> StudentCodes=>Set<StudentCode>();
    public DbSet<Permission> Permissions=>Set<Permission>();
    public DbSet<UserPermission> UserPermissions=>Set<UserPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity=>
    {
        entity.HasKey(e=>e.UserID);
        entity.HasIndex(e=>e.Username).IsUnique();
        entity.HasIndex(e=>e.Email).IsUnique();
        entity.Property(e=>e.Username).HasMaxLength(50).IsRequired();
        entity.Property(e=>e.Password).HasMaxLength(255).IsRequired();
        entity.Property(e=>e.FullName).HasMaxLength(100).IsRequired();
        entity.Property(e=>e.Email).HasMaxLength(100).IsRequired();
        entity.Property(e=>e.UserType).HasMaxLength(20).IsRequired();
    });

    modelBuilder.Entity<Course>(entity=>
    {
        entity.HasKey(e=>e.CourseID);
        entity.Property(e=>e.CourseName).HasMaxLength(100).IsRequired();
        entity.HasOne(e=>e.Professor)
              .WithMany(e=>e.CoursesAsProfessor)
              .HasForeignKey(e=>e.ProfessorID)
              .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<Enrollment>(entity=>
    {
        entity.HasKey(e=>e.EnrollmentID);
        entity.HasOne(e=>e.Student)
              .WithMany(e=>e.Enrollments)
              .HasForeignKey(e=>e.StudentID)
              .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e=>e.Course)
              .WithMany(e=>e.Enrollments)
              .HasForeignKey(e=>e.CourseID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Exam>(entity=>
    {
        entity.HasKey(e=>e.ExamID);
        entity.Property(e=>e.ExamName).HasMaxLength(100).IsRequired();
        entity.Property(e=>e.MaxScore).HasPrecision(5,2);
        entity.HasOne(e=>e.Course)
            .WithMany(e=>e.Exams)
            .HasForeignKey(e=>e.CourseID)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<ExamQuestion>(entity=>
    {
        entity.HasKey(e=>e.QuestionID);
        entity.Property(e=>e.QuestionType).HasMaxLength(30).IsRequired();
        entity.Property(e=>e.QuestionText).IsRequired();
        entity.HasOne(e=>e.Exam)
              .WithMany(e=>e.Questions)
              .HasForeignKey(e=>e.ExamID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<ExamOption>(entity=>
    {
        entity.HasKey(e=>e.OptionID);
        entity.Property(e=>e.OptionText).IsRequired();
        entity.HasOne(e=>e.Question)
              .WithMany(e=>e.Options)
              .HasForeignKey(e=>e.QuestionID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<StudentAnswer>(entity=>
    {
        entity.HasKey(e=>e.AnswerID);
        entity.Property(e=>e.Grade).HasPrecision(5,2);
        entity.HasOne(e=>e.Question)
              .WithMany(e=>e.StudentAnswers)
              .HasForeignKey(e=>e.QuestionID)
              .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e=>e.Student)
              .WithMany(e=>e.StudentAnswers)
              .HasForeignKey(e=>e.StudentID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Assignment>(entity=>
    {
        entity.HasKey(e=>e.AssignmentID);
        entity.Property(e=>e.AssignmentName).HasMaxLength(100).IsRequired();
        entity.HasOne(e=>e.Course)
              .WithMany(e=>e.Assignments)
              .HasForeignKey(e=>e.CourseID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<AssignmentSubmission>(entity=>
    {
        entity.HasKey(e=>e.SubmissionID);
        entity.Property(e=>e.AssignmentName).HasMaxLength(255).IsRequired();
        entity.Property(e=>e.SubmissionFileName).HasMaxLength(255).IsRequired();
        entity.Property(e=>e.Grade).HasPrecision(5,2);
        entity.HasOne(e=>e.Course)
              .WithMany(e=>e.AssignmentSubmissions)
              .HasForeignKey(e=>e.CourseID)
              .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e=>e.Student)
              .WithMany(e=>e.AssignmentSubmissions)
              .HasForeignKey(e=>e.StudentID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Grade>(entity=>
    {
        entity.HasKey(e=>e.GradeID);
        entity.HasIndex(e=>new{e.StudentID,e.CourseID}).IsUnique();
        entity.Property(e=>e.AssignmentGrade).HasPrecision(5,2);
        entity.Property(e=>e.ExamGrade).HasPrecision(5,2);
        entity.Property(e=>e.FinalGrade).HasPrecision(5,2);
        entity.Property(e=>e.TotalGrade).HasPrecision(5,2);
        entity.HasOne(e=>e.Student)
              .WithMany(e=>e.Grades)
              .HasForeignKey(e=>e.StudentID)
              .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e=>e.Course)
              .WithMany(e=>e.Grades)
              .HasForeignKey(e=>e.CourseID)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<StaffCode>(entity=>
    {
        entity.HasKey(e=>e.CodeID);
        entity.HasIndex(e=>e.FixedCode).IsUnique();
        entity.Property(e=>e.FixedCode).HasMaxLength(50).IsRequired();
    });

    modelBuilder.Entity<ProfessorCode>(entity=>
    {
        entity.HasKey(e=>e.CodeID);
        entity.HasIndex(e=>e.FixedCode).IsUnique();
        entity.Property(e=>e.FixedCode).HasMaxLength(50).IsRequired();
    });

    modelBuilder.Entity<StudentCode>(entity=>
    {
        entity.HasKey(e=>e.CodeID);
        entity.HasIndex(e=>e.FixedCode).IsUnique();
        entity.Property(e=>e.FixedCode).HasMaxLength(50).IsRequired();
    });

    modelBuilder.Entity<Course>(entity=>
    {
        entity.HasIndex(e=>e.CourseCode).IsUnique();
        entity.Property(e=>e.CourseCode).HasMaxLength(30).IsRequired();
        entity.Property(e=>e.ClassTime).HasMaxLength(80);
        entity.Property(e=>e.ClassLocation).HasMaxLength(120);
        entity.Property(e=>e.AcademicYear).HasMaxLength(20);
    });

    modelBuilder.Entity<Permission>(entity=>
    {
        entity.HasIndex(e=>e.Code).IsUnique();
        entity.Property(e=>e.Code).HasMaxLength(80).IsRequired();
        entity.Property(e=>e.Description).HasMaxLength(200).IsRequired();
        entity.Property(e=>e.Category).HasMaxLength(50).IsRequired();
    });

    modelBuilder.Entity<UserPermission>(entity=>
    {
        entity.HasIndex(e=>new{e.UserID,e.PermissionID}).IsUnique();
        entity.HasOne(e=>e.User)
            .WithMany(e=>e.UserPermissions)
            .HasForeignKey(e=>e.UserID)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e=>e.Permission)
            .WithMany(e=>e.UserPermissions)
            .HasForeignKey(e=>e.PermissionID)
            .OnDelete(DeleteBehavior.Cascade);
    });
}
}