namespace UniversityManagementSystem.Models;

public class LoginViewModel
{
    public string Username{get;set;}=string.Empty;
    public string Password{get;set;}=string.Empty;
    public string Role{get;set;}=string.Empty;
}

public class RegisterViewModel
{
    public string Username{get;set;}=string.Empty;
    public string Password{get;set;}=string.Empty;
    public string FullName{get;set;}=string.Empty;
    public string Email{get;set;}=string.Empty;
    public string Role{get;set;}=string.Empty;
    public string? FixedCode{get;set;}
}

public class UserCreateViewModel
{
    public string Username{get;set;}=string.Empty;
    public string Password{get;set;}=string.Empty;
    public string FullName{get;set;}=string.Empty;
    public string Email{get;set;}=string.Empty;
    public string Role{get;set;}=string.Empty;
}

public class UserListItemViewModel
{
    public int UserID{get;set;}
    public string Username{get;set;}=string.Empty;
    public string FullName{get;set;}=string.Empty;
    public string Email{get;set;}=string.Empty;
    public string UserType{get;set;}=string.Empty;
    public bool IsSuperAdmin{get;set;}
    public int PermissionCount{get;set;}
}

public class CourseListItemViewModel
{
    public int CourseID{get;set;}
    public string CourseCode{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public string? ProfessorName{get;set;}
    public string? AcademicYear{get;set;}
    public int StudentCount{get;set;}
}

public class CourseCreateViewModel
{
    public string CourseCode{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public int? ProfessorID{get;set;}
    public string? ClassTime{get;set;}
    public string? ClassLocation{get;set;}
    public DateTime? StartDate{get;set;}
    public DateTime? EndDate{get;set;}
    public DateTime? FinalExamDate{get;set;}
    public string? AcademicYear{get;set;}
    public List<int> StudentIDs{get;set;}=new List<int>();
}

public class CourseDetailsViewModel
{
    public int CourseID{get;set;}
    public string CourseCode{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public string? ProfessorName{get;set;}
    public string? ClassTime{get;set;}
    public string? ClassLocation{get;set;}
    public DateTime? StartDate{get;set;}
    public DateTime? EndDate{get;set;}
    public DateTime? FinalExamDate{get;set;}
    public string? AcademicYear{get;set;}
    public List<string> Students{get;set;}=new List<string>();
}

public class ProfessorOptionViewModel
{
    public int UserID{get;set;}
    public string FullName{get;set;}=string.Empty;
}

public class StudentOptionViewModel
{
    public int UserID{get;set;}
    public string FullName{get;set;}=string.Empty;
}

public class ExamListItemViewModel
{
    public int ExamID{get;set;}
    public string ExamName{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public DateTime? StartTime{get;set;}
    public DateTime? EndTime{get;set;}
    public int QuestionCount{get;set;}
    public decimal? MaxScore{get;set;}
}

public class ExamCreateViewModel
{
    public int CourseID{get;set;}
    public string ExamName{get;set;}=string.Empty;
    public decimal? MaxScore{get;set;}=20m;
    public DateTime? StartTime{get;set;}
    public DateTime? EndTime{get;set;}
    public List<QuestionCreateViewModel> Questions{get;set;}=new List<QuestionCreateViewModel>();
}

public class QuestionCreateViewModel
{
    public int? QuestionID{get;set;}
    public string QuestionType{get;set;}="MultipleChoice";
    public string QuestionText{get;set;}=string.Empty;
    public List<OptionCreateViewModel> Options{get;set;}=new List<OptionCreateViewModel>();
}

public class OptionCreateViewModel
{
    public int? OptionID{get;set;}
    public string OptionText{get;set;}=string.Empty;
    public bool IsCorrect{get;set;}
}

public class ExamAnswersCourseViewModel
{
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
}

public class ExamAnswersExamViewModel
{
    public int ExamID{get;set;}
    public string ExamName{get;set;}=string.Empty;
}

public class ExamAnswersStudentViewModel
{
    public int UserID{get;set;}
    public string FullName{get;set;}=string.Empty;
    public bool HasAnswered{get;set;}
}

public class StudentAnswerItemViewModel
{
    public int AnswerID{get;set;}
    public string QuestionText{get;set;}=string.Empty;
    public string QuestionType{get;set;}=string.Empty;
    public string? AnswerText{get;set;}
    public decimal? Grade{get;set;}
}

public class AssignmentListItemViewModel
{
    public int SubmissionID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public string StudentName{get;set;}=string.Empty;
    public string AssignmentName{get;set;}=string.Empty;
    public string SubmissionFileName{get;set;}=string.Empty;
    public decimal? Grade{get;set;}
}

public class AssignmentGradeViewModel
{
    public int SubmissionID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public string StudentName{get;set;}=string.Empty;
    public string AssignmentName{get;set;}=string.Empty;
    public string SubmissionFileName{get;set;}=string.Empty;
    public decimal? Grade{get;set;}
}

public class EditGradesCourseViewModel
{
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
}

public class EditGradesRowViewModel
{
    public int? GradeID{get;set;}
    public int StudentID{get;set;}
    public string StudentName{get;set;}=string.Empty;
    public decimal? AssignmentGrade{get;set;}
    public decimal? ExamGrade{get;set;}
    public decimal? FinalGrade{get;set;}
}

public class EditGradesViewModel
{
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public List<EditGradesRowViewModel> Rows{get;set;}=new List<EditGradesRowViewModel>();
}

public class StudentCourseViewModel
{
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public string? ProfessorName{get;set;}
}

public class StudentExamViewModel
{
    public int ExamID{get;set;}
    public string ExamName{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public DateTime? StartTime{get;set;}
    public DateTime? EndTime{get;set;}
    public bool IsOpen{get;set;}
    public bool HasTaken{get;set;}
}

public class TakeExamOptionViewModel
{
    public int OptionID{get;set;}
    public string OptionText{get;set;}=string.Empty;
}

public class TakeExamQuestionViewModel
{
    public int QuestionID{get;set;}
    public string QuestionType{get;set;}=string.Empty;
    public string QuestionText{get;set;}=string.Empty;
    public List<TakeExamOptionViewModel> Options{get;set;}=new List<TakeExamOptionViewModel>();
}

public class TakeExamViewModel
{
    public int ExamID{get;set;}
    public string ExamName{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public DateTime? EndTime{get;set;}
    public List<TakeExamQuestionViewModel> Questions{get;set;}=new List<TakeExamQuestionViewModel>();
}

public class StudentGradeViewModel
{
    public string CourseName{get;set;}=string.Empty;
    public decimal? ExamGrade{get;set;}
    public decimal? AssignmentGrade{get;set;}
    public decimal? TotalGrade{get;set;}
}

public class SubmitAssignmentViewModel
{
    public int CourseID{get;set;}
    public string AssignmentName{get;set;}=string.Empty;
    public IFormFile? File{get;set;}
    public List<StudentCourseViewModel> Courses{get;set;}=new List<StudentCourseViewModel>();
}

public class MyAssignmentViewModel
{
    public int SubmissionID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public string AssignmentName{get;set;}=string.Empty;
    public string SubmissionFileName{get;set;}=string.Empty;
    public decimal? Grade{get;set;}
}

public class PermissionItemViewModel
{
    public int PermissionID{get;set;}
    public string Code{get;set;}=string.Empty;
    public string Description{get;set;}=string.Empty;
    public string Category{get;set;}=string.Empty;
    public bool IsSelected{get;set;}
}

public class PermissionCategoryViewModel
{
    public string Category{get;set;}=string.Empty;
    public string CategoryLabel{get;set;}=string.Empty;
    public List<PermissionItemViewModel> Permissions{get;set;}=new List<PermissionItemViewModel>();
}

public class UserPermissionsViewModel
{
    public int UserID{get;set;}
    public string Username{get;set;}=string.Empty;
    public string FullName{get;set;}=string.Empty;
    public string UserType{get;set;}=string.Empty;
    public bool IsSuperAdmin{get;set;}
    public bool CanEdit{get;set;}
    public string? InfoMessage{get;set;}
    public List<PermissionCategoryViewModel> Categories{get;set;}=new List<PermissionCategoryViewModel>();
}

public class ReportsCourseViewModel
{
    public int CourseID{get;set;}
    public string CourseCode{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public string? ProfessorName{get;set;}
    public string? AcademicYear{get;set;}
    public int StudentCount{get;set;}
}

public class ReportsStudentGradeRowViewModel
{
    public int StudentID{get;set;}
    public string StudentName{get;set;}=string.Empty;
    public string StudentUsername{get;set;}=string.Empty;
    public decimal? ExamGrade{get;set;}
    public decimal? AssignmentGrade{get;set;}
    public decimal? FinalGrade{get;set;}
}

public class ReportsCourseGradesViewModel
{
    public int CourseID{get;set;}
    public string CourseCode{get;set;}=string.Empty;
    public string CourseName{get;set;}=string.Empty;
    public string? ProfessorName{get;set;}
    public string? AcademicYear{get;set;}
    public List<ReportsStudentGradeRowViewModel> Rows{get;set;}=new List<ReportsStudentGradeRowViewModel>();
    public decimal? AverageExamGrade{get;set;}
    public decimal? AverageAssignmentGrade{get;set;}
    public decimal? AverageFinalGrade{get;set;}
}

public class BulkGradeQuestionViewModel
{
    public int QuestionID{get;set;}
    public string QuestionText{get;set;}=string.Empty;
    public string QuestionType{get;set;}=string.Empty;
    public string QuestionTypeLabel{get;set;}=string.Empty;
}

public class BulkGradeCellViewModel
{
    public int AnswerID{get;set;}
    public string? AnswerText{get;set;}
    public decimal? Grade{get;set;}
}

public class BulkGradeStudentViewModel
{
    public int StudentID{get;set;}
    public string StudentName{get;set;}=string.Empty;
    public Dictionary<int,BulkGradeCellViewModel> Cells{get;set;}=new Dictionary<int,BulkGradeCellViewModel>();
}

public class BulkGradeViewModel
{
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public int ExamID{get;set;}
    public string ExamName{get;set;}=string.Empty;
    public List<BulkGradeQuestionViewModel> Questions{get;set;}=new List<BulkGradeQuestionViewModel>();
    public List<BulkGradeStudentViewModel> Students{get;set;}=new List<BulkGradeStudentViewModel>();
    public List<ExamAnswersCourseViewModel> Courses{get;set;}=new List<ExamAnswersCourseViewModel>();
    public List<ExamAnswersExamViewModel> Exams{get;set;}=new List<ExamAnswersExamViewModel>();
}

public class ExamEditViewModel
{
    public int ExamID{get;set;}
    public int CourseID{get;set;}
    public string CourseName{get;set;}=string.Empty;
    public string ExamName{get;set;}=string.Empty;
    public decimal? MaxScore{get;set;}
    public DateTime? StartTime{get;set;}
    public DateTime? EndTime{get;set;}
    public bool HasAnswers{get;set;}
    public List<QuestionCreateViewModel> Questions{get;set;}=new List<QuestionCreateViewModel>();
}