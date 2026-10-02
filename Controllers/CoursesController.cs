using Microsoft.AspNetCore.Mvc;
using UniversityManagementSystem.Data;
using UniversityManagementSystem.Filters;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers;

public class CoursesController:Controller
{
    private readonly AppDbContext _db;

    public CoursesController(AppDbContext db)
    {
        _db=db;
    }

    [PermissionAuthorize("Courses.View")]
    [HttpGet]
    public IActionResult Index()
    {
        var courses=_db.Courses
            .OrderBy(c=>c.CourseCode)
            .Select(c=>new CourseListItemViewModel
            {
                CourseID=c.CourseID,
                CourseCode=c.CourseCode,
                CourseName=c.CourseName,
                ProfessorName=c.Professor!=null ? c.Professor.FullName : null,
                AcademicYear=c.AcademicYear,
                StudentCount=c.Enrollments.Count
            })
            .ToList();

        return View(courses);
    }

    [PermissionAuthorize("Courses.View")]
    [HttpGet]
    public IActionResult Details(int id)
    {
        var model=_db.Courses
            .Where(c=>c.CourseID==id)
            .Select(c=>new CourseDetailsViewModel
            {
                CourseID=c.CourseID,
                CourseCode=c.CourseCode,
                CourseName=c.CourseName,
                ProfessorName=c.Professor!=null ? c.Professor.FullName : null,
                ClassTime=c.ClassTime,
                ClassLocation=c.ClassLocation,
                StartDate=c.StartDate,
                EndDate=c.EndDate,
                FinalExamDate=c.FinalExamDate,
                AcademicYear=c.AcademicYear,
                Students=c.Enrollments
                    .OrderBy(e=>e.Student!.FullName)
                    .Select(e=>e.Student!.FullName)
                    .ToList()
            })
            .FirstOrDefault();

        if (model==null)
        {
            return RedirectToAction("Index");
        }

        return View(model);
    }

    [PermissionAuthorize("Courses.Create")]
    [HttpGet]
    public IActionResult Create()
    {
        LoadDropdowns();
        return View(new CourseCreateViewModel());
    }

    [PermissionAuthorize("Courses.Create")]
    [HttpPost]
    public IActionResult Create(CourseCreateViewModel model)
    {
        LoadDropdowns();

        if (string.IsNullOrEmpty(model.CourseCode))
        {
            ViewBag.Error="کد درس الزامی است.";
            return View(model);
        }

        if (string.IsNullOrEmpty(model.CourseName))
        {
            ViewBag.Error="نام درس الزامی است.";
            return View(model);
        }

        if (_db.Courses.Any(c=>c.CourseCode==model.CourseCode))
        {
            ViewBag.Error="این کد درس قبلاً استفاده شده است.";
            return View(model);
        }

        if (model.ProfessorID==null)
        {
            ViewBag.Error="انتخاب استاد الزامی است.";
            return View(model);
        }

        if (model.StudentIDs==null || model.StudentIDs.Count==0)
        {
            ViewBag.Error="حداقل یک دانشجو باید انتخاب شود.";
            return View(model);
        }

        if (model.StartDate!=null && model.EndDate!=null && model.StartDate>model.EndDate)
        {
            ViewBag.Error="تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.";
            return View(model);
        }

        if (model.EndDate!=null && model.FinalExamDate!=null && model.FinalExamDate<model.EndDate)
        {
            ViewBag.Error="تاریخ امتحان نهایی نمی‌تواند قبل از تاریخ پایان کلاس باشد.";
            return View(model);
        }

        var professorExists=_db.Users.Any(u=>u.UserID==model.ProfessorID && u.UserType=="Professor");
        if (!professorExists)
        {
            ViewBag.Error="استاد انتخاب‌شده معتبر نیست.";
            return View(model);
        }

        var validStudentIds=_db.Users
            .Where(u=>u.UserType=="Student" && model.StudentIDs.Contains(u.UserID))
            .Select(u=>u.UserID)
            .ToList();

        if (validStudentIds.Count!=model.StudentIDs.Count)
        {
            ViewBag.Error="برخی دانشجویان انتخاب‌شده معتبر نیستند.";
            return View(model);
        }

        var course=new Course
        {
            CourseCode=model.CourseCode,
            CourseName=model.CourseName,
            ProfessorID=model.ProfessorID,
            ClassTime=model.ClassTime,
            ClassLocation=model.ClassLocation,
            StartDate=model.StartDate,
            EndDate=model.EndDate,
            FinalExamDate=model.FinalExamDate,
            AcademicYear=model.AcademicYear
        };

        _db.Courses.Add(course);
        _db.SaveChanges();

        foreach (var studentId in validStudentIds)
        {
            _db.Enrollments.Add(new Enrollment
            {
                StudentID=studentId,
                CourseID=course.CourseID
            });
        }

        _db.SaveChanges();

        return RedirectToAction("Index");
    }

    [PermissionAuthorize("Courses.Delete")]
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var course=_db.Courses.FirstOrDefault(c=>c.CourseID==id);
        if (course==null)
        {
            return RedirectToAction("Index");
        }

        var enrollments=_db.Enrollments.Where(e=>e.CourseID==id).ToList();
        _db.Enrollments.RemoveRange(enrollments);

        _db.Courses.Remove(course);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }

    private void LoadDropdowns()
    {
        var professors=_db.Users
            .Where(u=>u.UserType=="Professor")
            .OrderBy(u=>u.FullName)
            .Select(u=>new ProfessorOptionViewModel
            {
                UserID=u.UserID,
                FullName=u.FullName
            })
            .ToList();

        var students=_db.Users
            .Where(u=>u.UserType=="Student")
            .OrderBy(u=>u.FullName)
            .Select(u=>new StudentOptionViewModel
            {
                UserID=u.UserID,
                FullName=u.FullName
            })
            .ToList();

        ViewBag.Professors=professors;
        ViewBag.Students=students;
    }
}