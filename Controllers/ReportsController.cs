using Microsoft.AspNetCore.Mvc;
using UniversityManagementSystem.Data;
using UniversityManagementSystem.Filters;
using UniversityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace UniversityManagementSystem.Controllers;

public class ReportsController:Controller
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db=db;
    }

    [PermissionAuthorize("Grades.ViewAll")]
    [HttpGet]
    public IActionResult AllGrades()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var role=HttpContext.Session.GetString("UserRole");
        var isSuperAdmin=HttpContext.Session.GetString("IsSuperAdmin")=="true";

        var query=_db.Courses.AsQueryable();

        if (role=="Professor" && !isSuperAdmin)
        {
            query=query.Where(c=>c.ProfessorID==userId);
        }

        var courses=query
            .OrderBy(c=>c.CourseCode)
            .Select(c=>new ReportsCourseViewModel
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

    [PermissionAuthorize("Grades.ViewAll")]
    [HttpGet]
    public IActionResult CourseGrades(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        var role=HttpContext.Session.GetString("UserRole");
        var isSuperAdmin=HttpContext.Session.GetString("IsSuperAdmin")=="true";

        var course=_db.Courses
            .Include(c=>c.Professor)
            .FirstOrDefault(c=>c.CourseID==id);

        if (course==null)
        {
            return RedirectToAction("AllGrades");
        }

        if (role=="Professor" && !isSuperAdmin && course.ProfessorID!=userId)
        {
            return RedirectToAction("Forbidden","Home");
        }

        var studentIds=_db.Enrollments
            .Where(e=>e.CourseID==id)
            .Select(e=>new
            {
                StudentID=e.StudentID!.Value,
                FullName=e.Student!.FullName,
                Username=e.Student.Username
            })
            .ToList();

        var grades=_db.Grades
            .Where(g=>g.CourseID==id)
            .ToList();

        var model=new ReportsCourseGradesViewModel
        {
            CourseID=course.CourseID,
            CourseCode=course.CourseCode,
            CourseName=course.CourseName,
            ProfessorName=course.Professor!=null ? course.Professor.FullName : null,
            AcademicYear=course.AcademicYear
        };

        foreach (var s in studentIds.OrderBy(x=>x.FullName))
        {
            var g=grades.FirstOrDefault(x=>x.StudentID==s.StudentID);
            model.Rows.Add(new ReportsStudentGradeRowViewModel
            {
                StudentID=s.StudentID,
                StudentName=s.FullName,
                StudentUsername=s.Username,
                ExamGrade=g?.ExamGrade,
                AssignmentGrade=g?.AssignmentGrade,
                FinalGrade=g?.FinalGrade
            });
        }

        var withFinal=model.Rows.Where(r=>r.FinalGrade!=null).ToList();
        var withExam=model.Rows.Where(r=>r.ExamGrade!=null).ToList();
        var withAssignment=model.Rows.Where(r=>r.AssignmentGrade!=null).ToList();

        model.AverageExamGrade=withExam.Count>0 ? Math.Round(withExam.Average(r=>r.ExamGrade!.Value),2) : null;
        model.AverageAssignmentGrade=withAssignment.Count>0 ? Math.Round(withAssignment.Average(r=>r.AssignmentGrade!.Value),2) : null;
        model.AverageFinalGrade=withFinal.Count>0 ? Math.Round(withFinal.Average(r=>r.FinalGrade!.Value),2) : null;

        return View(model);
    }
}