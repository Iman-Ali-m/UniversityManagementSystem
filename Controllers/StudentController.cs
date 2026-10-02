using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Data;
using UniversityManagementSystem.Filters;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers;

public class StudentController:Controller
{
    private readonly AppDbContext _db;

    public StudentController(AppDbContext db)
    {
        _db=db;
    }

    [PermissionAuthorize]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [PermissionAuthorize("Student.Courses.View")]
    [HttpGet]
    public IActionResult Courses()
    {
        var userId=HttpContext.Session.GetInt32("UserId");

        var courses=_db.Enrollments
            .Where(e=>e.StudentID==userId)
            .OrderBy(e=>e.Course!.CourseName)
            .Select(e=>new StudentCourseViewModel
            {
                CourseID=e.CourseID!.Value,
                CourseName=e.Course!.CourseName,
                ProfessorName=e.Course.Professor!=null ? e.Course.Professor.FullName : null
            })
            .ToList();

        return View(courses);
    }

    [PermissionAuthorize("Student.Exams.View")]
    [HttpGet]
    public IActionResult Exams()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var now=DateTime.Now;

        var courseIds=_db.Enrollments
            .Where(e=>e.StudentID==userId)
            .Select(e=>e.CourseID!.Value)
            .ToHashSet();

        var exams=_db.Exams
            .Include(e=>e.Course)
            .Where(e=>courseIds.Contains(e.CourseID!.Value) && e.EndTime>now)
            .OrderBy(e=>e.StartTime)
            .ToList();

        var takenExamIds=_db.StudentAnswers
            .Where(sa=>sa.StudentID==userId)
            .Select(sa=>sa.Question!.ExamID!.Value)
            .Distinct()
            .ToHashSet();

        var model=exams.Select(e=>new StudentExamViewModel
        {
            ExamID=e.ExamID,
            ExamName=e.ExamName,
            CourseName=e.Course!=null ? e.Course.CourseName : "بدون درس",
            StartTime=e.StartTime,
            EndTime=e.EndTime,
            IsOpen=e.StartTime!=null && e.EndTime!=null && e.StartTime<=now && now<=e.EndTime,
            HasTaken=takenExamIds.Contains(e.ExamID)
        }).ToList();

        return View(model);
    }

    [PermissionAuthorize("Student.Exams.Take")]
    [HttpGet]
    public IActionResult TakeExam(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var now=DateTime.Now;

        var exam=_db.Exams
            .Include(e=>e.Course)
            .FirstOrDefault(e=>e.ExamID==id);

        if (exam==null || exam.Course==null)
        {
            return RedirectToAction("Exams");
        }

        var isEnrolled=_db.Enrollments.Any(e=>e.StudentID==userId && e.CourseID==exam.CourseID);
        if (!isEnrolled)
        {
            return RedirectToAction("Exams");
        }

        if (exam.StartTime==null || exam.EndTime==null)
        {
            return RedirectToAction("Exams");
        }

        if (now<exam.StartTime || now>exam.EndTime)
        {
            return RedirectToAction("Exams");
        }

        var questionIds=_db.ExamQuestions
            .Where(q=>q.ExamID==id)
            .Select(q=>q.QuestionID)
            .ToList();

        var alreadyTaken=_db.StudentAnswers
            .Any(sa=>sa.StudentID==userId && questionIds.Contains(sa.QuestionID!.Value));

        if (alreadyTaken)
        {
            return RedirectToAction("Exams");
        }

        var questions=_db.ExamQuestions
            .Where(q=>q.ExamID==id)
            .OrderBy(q=>q.QuestionID)
            .Select(q=>new TakeExamQuestionViewModel
            {
                QuestionID=q.QuestionID,
                QuestionType=q.QuestionType,
                QuestionText=q.QuestionText,
                Options=q.Options.Select(o=>new TakeExamOptionViewModel
                {
                    OptionID=o.OptionID,
                    OptionText=o.OptionText
                }).ToList()
            })
            .ToList();

        var model=new TakeExamViewModel
        {
            ExamID=exam.ExamID,
            ExamName=exam.ExamName,
            CourseName=exam.Course.CourseName,
            EndTime=exam.EndTime,
            Questions=questions
        };

        return View(model);
    }

    [PermissionAuthorize("Student.Exams.Take")]
    [HttpPost]
    public IActionResult TakeExam(int examId,Dictionary<int,string>? answer)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var now=DateTime.Now;

        var exam=_db.Exams
            .Include(e=>e.Course)
            .FirstOrDefault(e=>e.ExamID==examId);

        if (exam==null || exam.Course==null)
        {
            return RedirectToAction("Exams");
        }

        var isEnrolled=_db.Enrollments.Any(e=>e.StudentID==userId && e.CourseID==exam.CourseID);
        if (!isEnrolled)
        {
            return RedirectToAction("Exams");
        }

        if (exam.StartTime==null || exam.EndTime==null)
        {
            return RedirectToAction("Exams");
        }

        if (now<exam.StartTime || now>exam.EndTime)
        {
            return RedirectToAction("Exams");
        }

        var questions=_db.ExamQuestions
            .Where(q=>q.ExamID==examId)
            .ToList();

        var questionIds=questions.Select(q=>q.QuestionID).ToList();

        var alreadyTaken=_db.StudentAnswers
            .Any(sa=>sa.StudentID==userId && questionIds.Contains(sa.QuestionID!.Value));

        if (alreadyTaken)
        {
            return RedirectToAction("Exams");
        }

        if (answer==null)
        {
            answer=new Dictionary<int,string>();
        }

        foreach (var q in questions)
        {
            if (!answer.ContainsKey(q.QuestionID))
            {
                continue;
            }
            var value=answer[q.QuestionID];
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }
            _db.StudentAnswers.Add(new StudentAnswer
            {
                QuestionID=q.QuestionID,
                StudentID=userId,
                AnswerText=value,
                Grade=null
            });
        }

        _db.SaveChanges();

        return RedirectToAction("Exams");
    }

    [PermissionAuthorize("Student.Grades.View")]
    [HttpGet]
    public IActionResult Grades()
    {
        var userId=HttpContext.Session.GetInt32("UserId");

        var list=_db.Grades
            .Where(g=>g.StudentID==userId)
            .Select(g=>new StudentGradeViewModel
            {
                CourseName=g.Course!.CourseName,
                ExamGrade=g.ExamGrade,
                AssignmentGrade=g.AssignmentGrade,
                TotalGrade=g.TotalGrade
            })
            .ToList();

        return View(list);
    }

    [PermissionAuthorize("Student.Assignments.Submit")]
    [HttpGet]
    public IActionResult SubmitAssignment()
    {
        var userId=HttpContext.Session.GetInt32("UserId");

        var courses=_db.Enrollments
            .Where(e=>e.StudentID==userId)
            .OrderBy(e=>e.Course!.CourseName)
            .Select(e=>new StudentCourseViewModel
            {
                CourseID=e.CourseID!.Value,
                CourseName=e.Course!.CourseName,
                ProfessorName=e.Course.Professor!=null ? e.Course.Professor.FullName : null
            })
            .ToList();

        if (courses.Count==0)
        {
            TempData["Error"]="شما در هیچ درسی ثبت‌نام نشده‌اید. ابتدا پرسنل آموزشی شما را در یک درس ثبت‌نام کند.";
            return RedirectToAction("Index");
        }

        var model=new SubmitAssignmentViewModel
        {
            Courses=courses
        };

        return View(model);
    }

    [PermissionAuthorize("Student.Assignments.Submit")]
    [HttpPost]
    public async Task<IActionResult> SubmitAssignment(SubmitAssignmentViewModel model)
    {
        var userId=HttpContext.Session.GetInt32("UserId");

        var courses=_db.Enrollments
            .Where(e=>e.StudentID==userId)
            .OrderBy(e=>e.Course!.CourseName)
            .Select(e=>new StudentCourseViewModel
            {
                CourseID=e.CourseID!.Value,
                CourseName=e.Course!.CourseName
            })
            .ToList();

        model.Courses=courses;

        if (string.IsNullOrEmpty(model.AssignmentName))
        {
            ViewBag.Error="نام تمرین الزامی است.";
            return View(model);
        }

        if (model.File==null || model.File.Length==0)
        {
            ViewBag.Error="انتخاب فایل الزامی است.";
            return View(model);
        }

        var allowedCourseIds=courses.Select(c=>c.CourseID).ToHashSet();
        if (!allowedCourseIds.Contains(model.CourseID))
        {
            ViewBag.Error="درس انتخاب‌شده معتبر نیست.";
            return View(model);
        }

        const long maxSize=10*1024*1024;
        if (model.File.Length>maxSize)
        {
            ViewBag.Error="حجم فایل نباید بیشتر از ۱۰ مگابایت باشد.";
            return View(model);
        }

        byte[] fileBytes;
        using (var stream=new MemoryStream())
        {
            await model.File.CopyToAsync(stream);
            fileBytes=stream.ToArray();
        }

        var submission=new AssignmentSubmission
        {
            CourseID=model.CourseID,
            StudentID=userId,
            AssignmentName=model.AssignmentName,
            SubmissionFile=fileBytes,
            SubmissionFileName=model.File.FileName,
            Grade=null
        };

        _db.AssignmentSubmissions.Add(submission);
        _db.SaveChanges();

        TempData["Success"]="تمرین با موفقیت ارسال شد.";
        return RedirectToAction("MyAssignments");
    }

    [PermissionAuthorize("Student.Assignments.View")]
    [HttpGet]
    public IActionResult MyAssignments()
    {
        var userId=HttpContext.Session.GetInt32("UserId");

        var list=_db.AssignmentSubmissions
            .Where(s=>s.StudentID==userId)
            .OrderByDescending(s=>s.SubmissionID)
            .Select(s=>new MyAssignmentViewModel
            {
                SubmissionID=s.SubmissionID,
                CourseName=s.Course!.CourseName,
                AssignmentName=s.AssignmentName,
                SubmissionFileName=s.SubmissionFileName,
                Grade=s.Grade
            })
            .ToList();

        return View(list);
    }

    [PermissionAuthorize("Student.Assignments.View")]
    [HttpGet]
    public IActionResult DownloadMySubmission(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");

        var submission=_db.AssignmentSubmissions
            .FirstOrDefault(s=>s.SubmissionID==id && s.StudentID==userId);

        if (submission==null)
        {
            return NotFound();
        }

        if (submission.SubmissionFile==null || submission.SubmissionFile.Length==0)
        {
            return NotFound();
        }

        return File(submission.SubmissionFile,"application/octet-stream",submission.SubmissionFileName);
    }
}