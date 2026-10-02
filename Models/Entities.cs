namespace UniversityManagementSystem.Models;

public class User
{
    public int UserID{get;set;}
    public string Username{get;set;}=string.Empty;
    public string Password{get;set;}=string.Empty;
    public string FullName{get;set;}=string.Empty;
    public string Email{get;set;}=string.Empty;
    public string UserType{get;set;}=string.Empty;
    public bool IsSuperAdmin{get;set;}
    public ICollection<Course> CoursesAsProfessor{get;set;}=new List<Course>();
    public ICollection<Enrollment> Enrollments{get;set;}=new List<Enrollment>();
    public ICollection<StudentAnswer> StudentAnswers{get;set;}=new List<StudentAnswer>();
    public ICollection<Grade> Grades{get;set;}=new List<Grade>();
    public ICollection<AssignmentSubmission> AssignmentSubmissions{get;set;}=new List<AssignmentSubmission>();
    public ICollection<UserPermission> UserPermissions{get;set;}=new List<UserPermission>();
}

public class Course
{
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public int? ProfessorID{get;set;}
    public string CourseCode{get;set;}=string.Empty;
    public string? ClassTime{get;set;}
    public string? ClassLocation{get;set;}
    public DateTime? StartDate{get;set;}
    public DateTime? EndDate{get;set;}
    public DateTime? FinalExamDate{get;set;}
    public string? AcademicYear{get;set;}
    public User? Professor{get;set;}
    public ICollection<Enrollment> Enrollments{get;set;}=new List<Enrollment>();
    public ICollection<Exam> Exams{get;set;}=new List<Exam>();
    public ICollection<Assignment> Assignments{get;set;}=new List<Assignment>();
    public ICollection<AssignmentSubmission> AssignmentSubmissions{get;set;}=new List<AssignmentSubmission>();
    public ICollection<Grade> Grades{get;set;}=new List<Grade>();
}

public class Enrollment
{
    public int EnrollmentID{get;set;}
    public int? StudentID{get;set;}
    public User? Student{get;set;}
    public int? CourseID{get;set;}
    public Course? Course{get;set;}
}

public class Exam
{
    public decimal? MaxScore{get;set;}
    public int ExamID{get;set;}
    public int? CourseID{get;set;}
    public Course? Course{get;set;}
    public string ExamName{get;set;}=string.Empty;
    public DateTime? StartTime{get;set;}
    public DateTime? EndTime{get;set;}
    public ICollection<ExamQuestion> Questions{get;set;}=new List<ExamQuestion>();
}

public class ExamQuestion
{
    public int QuestionID{get;set;}
    public int? ExamID{get;set;}
    public Exam? Exam{get;set;}
    public string QuestionType{get;set;}=string.Empty;
    public string QuestionText{get;set;}=string.Empty;
    public string? CorrectAnswer{get;set;}
    public ICollection<ExamOption> Options{get;set;}=new List<ExamOption>();
    public ICollection<StudentAnswer> StudentAnswers{get;set;}=new List<StudentAnswer>();
}

public class ExamOption
{
    public int OptionID{get;set;}
    public int? QuestionID{get;set;}
    public ExamQuestion? Question{get;set;}
    public string OptionText{get;set;}=string.Empty;
    public bool IsCorrect{get;set;}
}

public class StudentAnswer
{
    public int AnswerID{get;set;}
    public int? QuestionID{get;set;}
    public ExamQuestion? Question{get;set;}
    public int? StudentID{get;set;}
    public User? Student{get;set;}
    public string? AnswerText{get;set;}
    public decimal? Grade{get;set;}
}

public class Assignment
{
    public int AssignmentID{get;set;}
    public int? CourseID{get;set;}
    public Course? Course{get;set;}
    public string AssignmentName{get;set;}=string.Empty;
    public string? Description{get;set;}
    public DateTime? Deadline{get;set;}
}

public class AssignmentSubmission
{
    public int SubmissionID{get;set;}
    public int? CourseID{get;set;}
    public Course? Course{get;set;}
    public int? StudentID{get;set;}
    public User? Student{get;set;}
    public string AssignmentName{get;set;}=string.Empty;
    public byte[] SubmissionFile{get;set;}=Array.Empty<byte>();
    public string SubmissionFileName{get;set;}=string.Empty;
    public decimal? Grade{get;set;}
}

public class Grade
{
    public int GradeID{get;set;}
    public int? StudentID{get;set;}
    public User? Student{get;set;}
    public int? CourseID{get;set;}
    public Course? Course{get;set;}
    public decimal? AssignmentGrade{get;set;}
    public decimal? ExamGrade{get;set;}
    public decimal? FinalGrade{get;set;}
    public decimal? TotalGrade{get;set;}
}

public class StaffCode
{
    public int CodeID{get;set;}
    public string FixedCode{get;set;}=string.Empty;
}

public class ProfessorCode
{
    public int CodeID{get;set;}
    public string FixedCode{get;set;}=string.Empty;
}

public class StudentCode
{
    public int CodeID{get;set;}
    public string FixedCode{get;set;}=string.Empty;
}

public class Permission
{
    public int PermissionID{get;set;}
    public string Code{get;set;}=string.Empty;
    public string Description{get;set;}=string.Empty;
    public string Category{get;set;}=string.Empty;
    public ICollection<UserPermission> UserPermissions{get;set;}=new List<UserPermission>();
}

public class UserPermission
{
    public int UserPermissionID{get;set;}
    public int UserID{get;set;}
    public User? User{get;set;}
    public int PermissionID{get;set;}
    public Permission? Permission{get;set;}
}