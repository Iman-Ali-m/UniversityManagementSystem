using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Data;
using UniversityManagementSystem.Filters;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers;

public class ProfessorController:Controller
{
    private const decimal MaxCourseGrade=20m;
    private readonly AppDbContext _db;
    private static readonly string[] ValidQuestionTypes={"MultipleChoice","TrueFalse","Descriptive","FillInTheBlank"};

    public ProfessorController(AppDbContext db)
    {
        _db=db;
    }

    [PermissionAuthorize]
    [HttpGet]
    public IActionResult Index() => View();

    [PermissionAuthorize("Exams.View")]
    [HttpGet]
    public IActionResult Exams()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var exams=_db.Exams.Where(e=>e.Course!.ProfessorID==userId).OrderByDescending(e=>e.StartTime)
            .Select(e=>new ExamListItemViewModel
            {
                ExamID=e.ExamID,ExamName=e.ExamName,CourseName=e.Course!.CourseName,MaxScore=e.MaxScore,
                StartTime=e.StartTime,EndTime=e.EndTime,QuestionCount=e.Questions.Count
            }).ToList();
        return View(exams);
    }

    [PermissionAuthorize("Exams.Create")]
    [HttpGet]
    public IActionResult CreateExam()
    {
        var courses=LoadProfessorCourses();
        if (courses.Count==0)
        {
            TempData["Error"]="شما هیچ درسی ندارید. ابتدا پرسنل آموزشی باید درسی به شما اختصاص دهد.";
            return RedirectToAction("Index");
        }
        ViewBag.Courses=courses;
        var model=new ExamCreateViewModel();
        model.Questions.Add(NewQuestion());
        return View(model);
    }

    [PermissionAuthorize("Exams.Create")]
    [HttpPost]
    public IActionResult CreateExam(ExamCreateViewModel model)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        ViewBag.Courses=LoadProfessorCourses();
        model.MaxScore=ClampMaxScore(model.MaxScore);
        var err=ValidateExam(model.CourseID,model.ExamName,model.StartTime,model.EndTime,model.Questions,userId);
        if (err!=null)
        {
            ViewBag.Error=err;
            model.Questions=NormalizeQuestions(model.Questions);
            return View(model);
        }
        var exam=new Exam
        {
            CourseID=model.CourseID,
            ExamName=model.ExamName,
            MaxScore=model.MaxScore,
            StartTime=model.StartTime,
            EndTime=model.EndTime
        };
        _db.Exams.Add(exam);
        _db.SaveChanges();
        SaveQuestions(exam.ExamID,model.Questions);
        TempData["Success"]="امتحان با موفقیت ساخته شد.";
        return RedirectToAction("Exams");
    }

    [PermissionAuthorize("Exams.Delete")]
    [HttpPost]
    public IActionResult DeleteExam(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var exam=_db.Exams.FirstOrDefault(e=>e.ExamID==id && e.Course!.ProfessorID==userId);
        if (exam!=null)
        {
            _db.Exams.Remove(exam);
            _db.SaveChanges();
        }
        return RedirectToAction("Exams");
    }

    [PermissionAuthorize("Exams.Create")]
    [HttpGet]
    public IActionResult EditExam(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var exam=_db.Exams.Include(e=>e.Course).FirstOrDefault(e=>e.ExamID==id && e.Course!.ProfessorID==userId);
        if (exam==null || exam.Course==null)
        {
            TempData["Error"]="امتحان انتخاب‌شده معتبر نیست.";
            return RedirectToAction("Exams");
        }
        var hasAnswers=_db.StudentAnswers.Any(sa=>sa.Question!.ExamID==id);
        var questions=_db.ExamQuestions.Where(q=>q.ExamID==id).OrderBy(q=>q.QuestionID)
            .Select(q=>new QuestionCreateViewModel
            {
                QuestionID=q.QuestionID,
                QuestionType=q.QuestionType,
                QuestionText=q.QuestionText,
                Options=q.Options.OrderBy(o=>o.OptionID).Select(o=>new OptionCreateViewModel
                {
                    OptionID=o.OptionID,
                    OptionText=o.OptionText,
                    IsCorrect=o.IsCorrect
                }).ToList()
            }).ToList();

        if (questions.Count==0)
            questions.Add(NewQuestion());

        var model=new ExamEditViewModel
        {
            ExamID=exam.ExamID,
            CourseID=exam.CourseID ?? 0,
            CourseName=exam.Course.CourseName,
            ExamName=exam.ExamName,
            StartTime=exam.StartTime,
            EndTime=exam.EndTime,
            MaxScore=exam.MaxScore ?? MaxCourseGrade,
            HasAnswers=hasAnswers,
            Questions=questions
        };
        return View(model);
    }

    [PermissionAuthorize("Exams.Create")]
    [HttpPost]
    public IActionResult EditExam(ExamEditViewModel model)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var exam=_db.Exams.Include(e=>e.Course).FirstOrDefault(e=>e.ExamID==model.ExamID && e.Course!.ProfessorID==userId);
        if (exam==null || exam.Course==null)
        {
            TempData["Error"]="امتحان انتخاب‌شده معتبر نیست.";
            return RedirectToAction("Exams");
        }
        model.CourseID=exam.CourseID ?? 0;
        model.CourseName=exam.Course.CourseName;
        model.HasAnswers=_db.StudentAnswers.Any(sa=>sa.Question!.ExamID==model.ExamID);
        model.MaxScore=ClampMaxScore(model.MaxScore);

        if (string.IsNullOrEmpty(model.ExamName) || model.StartTime==null || model.EndTime==null || model.EndTime<=model.StartTime)
        {
            ViewBag.Error=ValidateExamMeta(model.ExamName,model.StartTime,model.EndTime);
            model.Questions=NormalizeQuestions(model.Questions);
            return View(model);
        }

        var answeredQIds=GetAnsweredQuestionIds(model.ExamID);
        var qerr=ValidateQuestions(model.Questions,answeredQIds);
        if (qerr!=null)
        {
            ViewBag.Error=qerr;
            model.Questions=NormalizeQuestions(model.Questions);
            return View(model);
        }

        exam.ExamName=model.ExamName;
        exam.StartTime=model.StartTime;
        exam.EndTime=model.EndTime;
        exam.MaxScore=model.MaxScore;
        _db.SaveChanges();

        SyncQuestions(model.ExamID,model.Questions,answeredQIds);
        TempData["Success"]="امتحان با موفقیت ویرایش شد.";
        return RedirectToAction("Exams");
    }

    [PermissionAuthorize("Exams.Grade")]
    [HttpGet]
    public IActionResult ExamAnswers(int? courseId,int? examId,int? studentId)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var courses=_db.Courses.Where(c=>c.ProfessorID==userId).OrderBy(c=>c.CourseName)
            .Select(c=>new ExamAnswersCourseViewModel{CourseID=c.CourseID,CourseName=c.CourseName}).ToList();

        ViewBag.Courses=courses;
        ViewBag.SelectedCourseId=courseId;
        ViewBag.SelectedExamId=examId;
        ViewBag.SelectedStudentId=studentId;

        var exams=new List<ExamAnswersExamViewModel>();
        if (courseId!=null && courses.Any(c=>c.CourseID==courseId))
            exams=_db.Exams.Where(e=>e.CourseID==courseId).OrderBy(e=>e.ExamName)
                .Select(e=>new ExamAnswersExamViewModel{ExamID=e.ExamID,ExamName=e.ExamName}).ToList();
        ViewBag.Exams=exams;

        var students=new List<ExamAnswersStudentViewModel>();
        if (courseId!=null && examId!=null && exams.Any(e=>e.ExamID==examId))
        {
            var answered=_db.StudentAnswers.Where(sa=>sa.Question!.ExamID==examId)
                .Select(sa=>sa.StudentID!.Value).Distinct().ToHashSet();
            students=_db.Enrollments.Where(en=>en.CourseID==courseId)
                .Select(en=>new{en.StudentID,en.Student!.FullName}).ToList()
                .Select(s=>new ExamAnswersStudentViewModel
                {
                    UserID=s.StudentID!.Value,FullName=s.FullName,
                    HasAnswered=answered.Contains(s.StudentID!.Value)
                }).OrderBy(s=>s.FullName).ToList();
        }
        ViewBag.Students=students;

        var answers=new List<StudentAnswerItemViewModel>();
        if (courseId!=null && examId!=null && studentId!=null &&
            exams.Any(e=>e.ExamID==examId) && students.Any(s=>s.UserID==studentId))
        {
            var raw=_db.StudentAnswers.Where(sa=>sa.StudentID==studentId && sa.Question!.ExamID==examId)
                .OrderBy(sa=>sa.QuestionID)
                .Select(sa=>new{sa.AnswerID,QuestionText=sa.Question!.QuestionText,
                    QuestionType=sa.Question.QuestionType,sa.AnswerText,sa.Grade}).ToList();

            var optIds=raw.Where(a=>a.QuestionType=="MultipleChoice" && int.TryParse(a.AnswerText,out _))
                .Select(a=>int.Parse(a.AnswerText!)).ToHashSet();
            var optTexts=_db.ExamOptions.Where(o=>optIds.Contains(o.OptionID))
                .ToDictionary(o=>o.OptionID,o=>o.OptionText);

            answers=raw.Select(a=>new StudentAnswerItemViewModel
            {
                AnswerID=a.AnswerID,QuestionText=a.QuestionText,QuestionType=a.QuestionType,
                AnswerText=FormatAnswerForDisplay(a.QuestionType,a.AnswerText,optTexts),Grade=a.Grade
            }).ToList();
        }
        ViewBag.Answers=answers;
        return View();
    }

    [PermissionAuthorize("Exams.Grade")]
    [HttpPost]
    public IActionResult GradeExam(int examId,int studentId,Dictionary<int,decimal?> grades)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var exam=_db.Exams.Include(e=>e.Course).FirstOrDefault(e=>e.ExamID==examId);
        if (exam==null || exam.Course==null || exam.Course.ProfessorID!=userId)
        {
            TempData["Error"]="امتحان انتخاب‌شده معتبر نیست.";
            return RedirectToAction("ExamAnswers");
        }
        var courseId=exam.Course.CourseID;
        if (!_db.Enrollments.Any(en=>en.StudentID==studentId && en.CourseID==courseId))
        {
            TempData["Error"]="این دانشجو در این درس ثبت‌نام نشده است.";
            return RedirectToAction("ExamAnswers",new{courseId,examId});
        }

        var answers=_db.StudentAnswers.Where(sa=>sa.StudentID==studentId && sa.Question!.ExamID==examId).ToList();
        foreach (var answer in answers)
            if (grades.ContainsKey(answer.AnswerID))
                answer.Grade=grades[answer.AnswerID] ?? 0m;
        _db.SaveChanges();

        RecalculateGrade(studentId,courseId,examId);
        TempData["Success"]="نمرات با موفقیت ذخیره شد.";
        return RedirectToAction("ExamAnswers",new{courseId,examId,studentId});
    }

    [PermissionAuthorize("Assignments.View")]
    [HttpGet]
    public IActionResult Assignments()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var items=_db.AssignmentSubmissions.Where(s=>s.Course!.ProfessorID==userId)
            .OrderBy(s=>s.Course!.CourseName).ThenBy(s=>s.Student!.FullName)
            .Select(s=>new AssignmentListItemViewModel
            {
                SubmissionID=s.SubmissionID,CourseName=s.Course!.CourseName,StudentName=s.Student!.FullName,
                AssignmentName=s.AssignmentName,SubmissionFileName=s.SubmissionFileName,Grade=s.Grade
            }).ToList();
        return View(items);
    }

    [PermissionAuthorize("Assignments.Grade")]
    [HttpGet]
    public IActionResult GradeAssignment(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var submission=_db.AssignmentSubmissions.Where(s=>s.SubmissionID==id && s.Course!.ProfessorID==userId)
            .Select(s=>new AssignmentGradeViewModel
            {
                SubmissionID=s.SubmissionID,CourseName=s.Course!.CourseName,StudentName=s.Student!.FullName,
                AssignmentName=s.AssignmentName,SubmissionFileName=s.SubmissionFileName,Grade=s.Grade
            }).FirstOrDefault();
        if (submission==null)
        {
            TempData["Error"]="تمرین انتخاب‌شده معتبر نیست.";
            return RedirectToAction("Assignments");
        }
        return View(submission);
    }

    [PermissionAuthorize("Assignments.Grade")]
    [HttpPost]
    public IActionResult GradeAssignment(int id,decimal? grade)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var submission=_db.AssignmentSubmissions.Include(s=>s.Course).FirstOrDefault(s=>s.SubmissionID==id);
        if (submission==null || submission.Course==null || submission.Course.ProfessorID!=userId)
        {
            TempData["Error"]="تمرین انتخاب‌شده معتبر نیست.";
            return RedirectToAction("Assignments");
        }
        if (grade==null || grade<0)
        {
            TempData["Error"]="نمره وارد شده معتبر نیست.";
            return RedirectToAction("GradeAssignment",new{id});
        }
        submission.Grade=grade.Value;
        _db.SaveChanges();
        RecalculateGrade(submission.StudentID!.Value,submission.CourseID!.Value,null);
        TempData["Success"]="نمره با موفقیت ذخیره شد.";
        return RedirectToAction("Assignments");
    }

    [PermissionAuthorize("Assignments.View")]
    [HttpGet]
    public IActionResult DownloadSubmission(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var s=_db.AssignmentSubmissions.Include(x=>x.Course).FirstOrDefault(x=>x.SubmissionID==id);
        if (s==null || s.Course==null || s.Course.ProfessorID!=userId) return NotFound();
        if (s.SubmissionFile==null || s.SubmissionFile.Length==0) return NotFound();
        return File(s.SubmissionFile,"application/octet-stream",s.SubmissionFileName);
    }

    [PermissionAuthorize("Grades.Edit")]
    [HttpGet]
    public IActionResult Grades()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var courses=_db.Courses.Where(c=>c.ProfessorID==userId).OrderBy(c=>c.CourseName)
            .Select(c=>new EditGradesCourseViewModel{CourseID=c.CourseID,CourseName=c.CourseName}).ToList();
        return View(courses);
    }

    [PermissionAuthorize("Grades.Edit")]
    [HttpGet]
    public IActionResult EditGrades(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var course=_db.Courses.FirstOrDefault(c=>c.CourseID==id && c.ProfessorID==userId);
        if (course==null)
        {
            TempData["Error"]="درس انتخاب‌شده معتبر نیست.";
            return RedirectToAction("Grades");
        }
        var studentIds=_db.Enrollments.Where(e=>e.CourseID==id)
            .Select(e=>new{e.StudentID,e.Student!.FullName}).ToList();
        var grades=_db.Grades.Where(g=>g.CourseID==id).ToList();
        var model=new EditGradesViewModel{CourseID=id,CourseName=course.CourseName};
        foreach (var s in studentIds.OrderBy(x=>x.FullName))
        {
            var g=grades.FirstOrDefault(x=>x.StudentID==s.StudentID);
            model.Rows.Add(new EditGradesRowViewModel
            {
                GradeID=g?.GradeID,StudentID=s.StudentID!.Value,StudentName=s.FullName,
                AssignmentGrade=g?.AssignmentGrade,ExamGrade=g?.ExamGrade,FinalGrade=g?.FinalGrade
            });
        }
        return View(model);
    }

    [PermissionAuthorize("Grades.Edit")]
    [HttpPost]
    public IActionResult EditGrades(EditGradesViewModel model)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var course=_db.Courses.FirstOrDefault(c=>c.CourseID==model.CourseID && c.ProfessorID==userId);
        if (course==null)
        {
            TempData["Error"]="درس انتخاب‌شده معتبر نیست.";
            return RedirectToAction("Grades");
        }
        model.Rows=model.Rows ?? new List<EditGradesRowViewModel>();
        var studentIds=_db.Enrollments.Where(e=>e.CourseID==model.CourseID)
            .Select(e=>e.StudentID!.Value).ToHashSet();
        var existing=_db.Grades.Where(g=>g.CourseID==model.CourseID).ToList();

        foreach (var row in model.Rows)
        {
            if (!studentIds.Contains(row.StudentID)) continue;
            decimal? a=row.AssignmentGrade; if (a!=null && a<0) a=0;
            decimal? e=row.ExamGrade; if (e!=null && e<0) e=0;
            decimal? f=row.FinalGrade ?? ((a ?? 0m)+(e ?? 0m));
            if (f<0) f=0;
            if (f>MaxCourseGrade) f=MaxCourseGrade;
            var rec=existing.FirstOrDefault(g=>g.StudentID==row.StudentID);
            if (rec==null)
                _db.Grades.Add(new Grade{StudentID=row.StudentID,CourseID=model.CourseID,
                    AssignmentGrade=a,ExamGrade=e,FinalGrade=f,TotalGrade=f});
            else { rec.AssignmentGrade=a; rec.ExamGrade=e; rec.FinalGrade=f; rec.TotalGrade=f; }
        }

        _db.SaveChanges();
        TempData["Success"]="نمرات با موفقیت به‌روزرسانی شد.";
        return RedirectToAction("EditGrades",new{id=model.CourseID});
    }

    [PermissionAuthorize("Exams.GradeBulk")]
    [HttpGet]
    public IActionResult ExamAnswersBulk(int? courseId,int? examId)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var courses=_db.Courses.Where(c=>c.ProfessorID==userId).OrderBy(c=>c.CourseName)
            .Select(c=>new ExamAnswersCourseViewModel{CourseID=c.CourseID,CourseName=c.CourseName}).ToList();
        var model=new BulkGradeViewModel{Courses=courses,CourseID=courseId ?? 0,ExamID=examId ?? 0};

        if (courseId==null || examId==null) return View(model);
        var course=courses.FirstOrDefault(c=>c.CourseID==courseId);
        if (course==null) return View(model);
        var exam=_db.Exams.Where(e=>e.ExamID==examId && e.CourseID==courseId)
            .Select(e=>new{e.ExamID,e.ExamName}).FirstOrDefault();
        if (exam==null) return View(model);

        model.CourseName=course.CourseName;
        model.ExamName=exam.ExamName;
        model.Exams=_db.Exams.Where(e=>e.CourseID==courseId).OrderBy(e=>e.ExamName)
            .Select(e=>new ExamAnswersExamViewModel{ExamID=e.ExamID,ExamName=e.ExamName}).ToList();

        var questions=_db.ExamQuestions.Where(q=>q.ExamID==examId).OrderBy(q=>q.QuestionID)
            .Select(q=>new BulkGradeQuestionViewModel
            {
                QuestionID=q.QuestionID,QuestionText=q.QuestionText,QuestionType=q.QuestionType
            }).ToList();
        foreach (var q in questions)
            q.QuestionTypeLabel=TranslateQuestionType(q.QuestionType);
        model.Questions=questions;

        var qIds=questions.Select(q=>q.QuestionID).ToHashSet();
        var answers=_db.StudentAnswers.Where(sa=>qIds.Contains(sa.QuestionID!.Value))
            .Select(sa=>new{sa.AnswerID,sa.QuestionID,sa.StudentID,sa.AnswerText,sa.Grade,
                StudentName=sa.Student!.FullName,QuestionType=sa.Question!.QuestionType}).ToList();
        var optTexts=_db.ExamOptions.Where(o=>qIds.Contains(o.QuestionID!.Value))
            .Select(o=>new{o.OptionID,o.OptionText}).ToList().ToDictionary(o=>o.OptionID,o=>o.OptionText);

        foreach (var g in answers.GroupBy(a=>new{a.StudentID,a.StudentName}).OrderBy(g=>g.Key.StudentName))
        {
            var row=new BulkGradeStudentViewModel{StudentID=g.Key.StudentID!.Value,StudentName=g.Key.StudentName};
            foreach (var q in questions)
            {
                var ans=g.FirstOrDefault(x=>x.QuestionID==q.QuestionID);
                if (ans==null) continue;
                row.Cells[q.QuestionID]=new BulkGradeCellViewModel
                {
                    AnswerID=ans.AnswerID,
                    AnswerText=FormatAnswerForDisplay(ans.QuestionType,ans.AnswerText,optTexts),
                    Grade=ans.Grade
                };
            }
            model.Students.Add(row);
        }
        return View(model);
    }

    [PermissionAuthorize("Exams.GradeBulk")]
    [HttpPost]
    public IActionResult ExamAnswersBulk(int examId,Dictionary<string,decimal?> values)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var exam=_db.Exams.Include(e=>e.Course).FirstOrDefault(e=>e.ExamID==examId);
        if (exam==null || exam.Course==null || exam.Course.ProfessorID!=userId)
        {
            TempData["Error"]="امتحان انتخاب‌شده معتبر نیست.";
            return RedirectToAction("ExamAnswersBulk");
        }
        values=values ?? new Dictionary<string,decimal?>();
        var all=_db.StudentAnswers.Where(sa=>sa.Question!.ExamID==examId).ToList();
        var touched=new HashSet<int>();

        foreach (var kv in values)
        {
            var parts=kv.Key.Split('_');
            if (parts.Length!=2) continue;
            if (!int.TryParse(parts[0],out int sid)) continue;
            if (!int.TryParse(parts[1],out int aid)) continue;
            var ans=all.FirstOrDefault(a=>a.AnswerID==aid && a.StudentID==sid);
            if (ans==null) continue;
            var val=kv.Value; if (val==null || val<0) val=0m;
            ans.Grade=val;
            touched.Add(sid);
        }
        _db.SaveChanges();
        foreach (var sid in touched)
            RecalculateGrade(sid,exam.Course.CourseID,examId);

        TempData["Success"]="نمرات گروهی با موفقیت ذخیره شد.";
        return RedirectToAction("ExamAnswersBulk",new{courseId=exam.Course.CourseID,examId});
    }

    private List<object> LoadProfessorCourses()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        return _db.Courses.Where(c=>c.ProfessorID==userId).OrderBy(c=>c.CourseName)
            .Select(c=>new{c.CourseID,c.CourseName}).ToList().Cast<object>().ToList();
    }

    private static QuestionCreateViewModel NewQuestion() => new QuestionCreateViewModel
    {
        QuestionType="MultipleChoice",
        Options=Enumerable.Range(0,4).Select(_=>new OptionCreateViewModel()).ToList()
    };

    private static decimal ClampMaxScore(decimal? value)
    {
        if (value==null || value<=0 || value>MaxCourseGrade) return MaxCourseGrade;
        return value.Value;
    }

    private string? ValidateExam(int courseId,string examName,DateTime? start,DateTime? end,List<QuestionCreateViewModel>? questions,int? userId)
    {
        if (!_db.Courses.Any(c=>c.CourseID==courseId && c.ProfessorID==userId))
            return "درس انتخاب‌شده معتبر نیست.";
        var meta=ValidateExamMeta(examName,start,end);
        if (meta!=null) return meta;
        return ValidateQuestions(questions,null);
    }

    private static string? ValidateExamMeta(string examName,DateTime? start,DateTime? end)
    {
        if (string.IsNullOrEmpty(examName)) return "نام امتحان الزامی است.";
        if (start==null || end==null) return "زمان شروع و پایان الزامی است.";
        if (end<=start) return "زمان پایان باید بعد از زمان شروع باشد.";
        return null;
    }

    private static string? ValidateQuestions(List<QuestionCreateViewModel>? questions,HashSet<int>? answeredIds)
    {
        if (questions==null || questions.Count==0) return "حداقل یک سوال باید وارد شود.";
        for (int i=0;i<questions.Count;i++)
        {
            var q=questions[i];
            if (string.IsNullOrEmpty(q.QuestionText)) return $"متن سوال {i+1} الزامی است.";
            if (!ValidQuestionTypes.Contains(q.QuestionType)) return $"نوع سوال {i+1} معتبر نیست.";
            var hasAns=q.QuestionID.HasValue && answeredIds!=null && answeredIds.Contains(q.QuestionID.Value);
            if (q.QuestionType=="MultipleChoice" && !hasAns)
            {
                var opts=q.Options.Where(o=>!string.IsNullOrEmpty(o.OptionText)).ToList();
                if (opts.Count<2) return $"سوال {i+1} باید حداقل دو گزینه داشته باشد.";
                if (!opts.Any(o=>o.IsCorrect)) return $"سوال {i+1} باید حداقل یک گزینه صحیح داشته باشد.";
            }
        }
        return null;
    }

    private static List<QuestionCreateViewModel> NormalizeQuestions(List<QuestionCreateViewModel>? questions)
    {
        var result=questions ?? new List<QuestionCreateViewModel>();
        foreach (var q in result)
        {
            q.Options=q.Options ?? new List<OptionCreateViewModel>();
            if (q.QuestionType=="MultipleChoice" && q.Options.Count==0)
                for (int j=0;j<4;j++) q.Options.Add(new OptionCreateViewModel());
        }
        return result;
    }

    private HashSet<int> GetAnsweredQuestionIds(int examId)
    {
        return _db.StudentAnswers.Where(sa=>sa.Question!.ExamID==examId)
            .Select(sa=>sa.QuestionID!.Value).Distinct().ToHashSet();
    }

    private void SaveQuestions(int examId,List<QuestionCreateViewModel> questions)
    {
        foreach (var q in questions)
        {
            var question=new ExamQuestion{ExamID=examId,QuestionType=q.QuestionType,QuestionText=q.QuestionText};
            _db.ExamQuestions.Add(question);
            _db.SaveChanges();
            if (q.QuestionType=="MultipleChoice")
            {
                foreach (var opt in q.Options)
                {
                    if (string.IsNullOrEmpty(opt.OptionText)) continue;
                    _db.ExamOptions.Add(new ExamOption
                    {
                        QuestionID=question.QuestionID,OptionText=opt.OptionText,IsCorrect=opt.IsCorrect
                    });
                }
                _db.SaveChanges();
            }
        }
    }

    private void SyncQuestions(int examId,List<QuestionCreateViewModel> submitted,HashSet<int> answeredQIds)
    {
        var existing=_db.ExamQuestions.Where(q=>q.ExamID==examId).ToList();
        var existingIds=existing.Select(q=>q.QuestionID).ToHashSet();
        var submittedIds=submitted.Where(q=>q.QuestionID.HasValue && q.QuestionID.Value>0)
            .Select(q=>q.QuestionID!.Value).ToHashSet();

        foreach (var oldQ in existing.Where(q=>!submittedIds.Contains(q.QuestionID)))
        {
            if (answeredQIds.Contains(oldQ.QuestionID)) continue;
            _db.ExamOptions.RemoveRange(_db.ExamOptions.Where(o=>o.QuestionID==oldQ.QuestionID));
            _db.ExamQuestions.Remove(oldQ);
        }
        _db.SaveChanges();

        var usedOptionIds=_db.StudentAnswers.Where(sa=>sa.Question!.ExamID==examId && sa.AnswerText!=null)
            .Select(sa=>sa.AnswerText!).ToList()
            .Where(t=>int.TryParse(t,out _)).Select(int.Parse).ToHashSet();

        foreach (var q in submitted)
        {
            var isNew=!q.QuestionID.HasValue || q.QuestionID.Value==0 || !existingIds.Contains(q.QuestionID.Value);
            if (isNew)
            {
                var newQ=new ExamQuestion{ExamID=examId,QuestionType=q.QuestionType,QuestionText=q.QuestionText};
                _db.ExamQuestions.Add(newQ);
                _db.SaveChanges();
                if (q.QuestionType=="MultipleChoice")
                {
                    foreach (var o in q.Options.Where(o=>!string.IsNullOrEmpty(o.OptionText)))
                        _db.ExamOptions.Add(new ExamOption
                        {
                            QuestionID=newQ.QuestionID,OptionText=o.OptionText,IsCorrect=o.IsCorrect
                        });
                    _db.SaveChanges();
                }
                continue;
            }

            var eq=existing.First(x=>x.QuestionID==q.QuestionID!.Value);
            var hasAns=answeredQIds.Contains(eq.QuestionID);
            eq.QuestionText=q.QuestionText;
            if (!hasAns && eq.QuestionType!=q.QuestionType) eq.QuestionType=q.QuestionType;
            _db.SaveChanges();

            if (eq.QuestionType=="MultipleChoice")
            {
                var dbOpts=_db.ExamOptions.Where(o=>o.QuestionID==eq.QuestionID).ToList();
                var dbOptIds=dbOpts.Select(o=>o.OptionID).ToHashSet();
                var subOptIds=q.Options.Where(o=>o.OptionID.HasValue && o.OptionID.Value>0)
                    .Select(o=>o.OptionID!.Value).ToHashSet();

                foreach (var oldO in dbOpts.Where(o=>!subOptIds.Contains(o.OptionID)))
                {
                    if (usedOptionIds.Contains(oldO.OptionID)) continue;
                    _db.ExamOptions.Remove(oldO);
                }
                _db.SaveChanges();

                foreach (var o in q.Options.Where(o=>!string.IsNullOrEmpty(o.OptionText)))
                {
                    if (o.OptionID.HasValue && o.OptionID.Value>0 && dbOptIds.Contains(o.OptionID.Value))
                    {
                        var eo=dbOpts.First(x=>x.OptionID==o.OptionID.Value);
                        eo.OptionText=o.OptionText;
                        eo.IsCorrect=o.IsCorrect;
                    }
                    else
                    {
                        _db.ExamOptions.Add(new ExamOption
                        {
                            QuestionID=eq.QuestionID,OptionText=o.OptionText,IsCorrect=o.IsCorrect
                        });
                    }
                }
                _db.SaveChanges();
            }
            else if (!hasAns)
            {
                _db.ExamOptions.RemoveRange(_db.ExamOptions.Where(o=>o.QuestionID==eq.QuestionID));
                _db.SaveChanges();
            }
        }
    }

    private void RecalculateGrade(int studentId,int courseId,int? examId)
    {
        decimal examGrade;
        if (examId.HasValue)
            examGrade=_db.StudentAnswers.Where(sa=>sa.StudentID==studentId && sa.Question!.ExamID==examId)
                .Sum(a=>(decimal?)a.Grade) ?? 0m;
        else
            examGrade=_db.Grades.FirstOrDefault(g=>g.StudentID==studentId && g.CourseID==courseId)?.ExamGrade ?? 0m;

        decimal assignmentGrade=_db.AssignmentSubmissions
            .Where(a=>a.StudentID==studentId && a.CourseID==courseId).Sum(a=>(decimal?)a.Grade) ?? 0m;

        decimal total=examGrade+assignmentGrade;
        if (total>MaxCourseGrade) total=MaxCourseGrade;
        if (total<0m) total=0m;

        var record=_db.Grades.FirstOrDefault(g=>g.StudentID==studentId && g.CourseID==courseId);
        if (record==null)
            _db.Grades.Add(new Grade{StudentID=studentId,CourseID=courseId,ExamGrade=examGrade,
                AssignmentGrade=assignmentGrade,FinalGrade=total,TotalGrade=total});
        else
        {
            record.ExamGrade=examGrade;
            record.AssignmentGrade=assignmentGrade;
            record.FinalGrade=total;
            record.TotalGrade=total;
        }
        _db.SaveChanges();
    }

    private static string TranslateQuestionType(string type) => type switch
    {
        "MultipleChoice"=>"چند گزینه‌ای",
        "TrueFalse"=>"صحیح / غلط",
        "Descriptive"=>"تشریحی",
        "FillInTheBlank"=>"جای خالی",
        _=>type
    };

    private static string? FormatAnswerForDisplay(string questionType,string? answerText,Dictionary<int,string> optionTexts)
    {
        if (string.IsNullOrEmpty(answerText)) return answerText;
        if (questionType=="MultipleChoice" && int.TryParse(answerText,out int optionId))
            return optionTexts.ContainsKey(optionId) ? optionTexts[optionId] : answerText;
        if (questionType=="TrueFalse")
            return answerText switch{"true"=>"صحیح","false"=>"غلط",_=>answerText};
        return answerText;
    }
}