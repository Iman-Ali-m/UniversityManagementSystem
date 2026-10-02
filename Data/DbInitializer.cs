using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.Migrate();

        SeedPermissions(db);
        SeedCodes(db);
        SeedAdmin(db);
        BackfillUserPermissions(db);
    }

    private static void SeedPermissions(AppDbContext db)
    {
        var existing=db.Permissions.Select(p=>p.Code).ToHashSet();
        var toAdd=AllPermissions.Where(p=>!existing.Contains(p.Code)).ToList();

        if (toAdd.Count>0)
        {
            db.Permissions.AddRange(toAdd);
            db.SaveChanges();
        }
    }

    private static void SeedCodes(AppDbContext db)
    {
        if (!db.StaffCodes.Any())
        {
            db.StaffCodes.Add(new StaffCode{FixedCode="STAFF001"});
            db.SaveChanges();
        }

        if (!db.ProfessorCodes.Any())
        {
            db.ProfessorCodes.Add(new ProfessorCode{FixedCode="PROF001"});
            db.SaveChanges();
        }

        if (!db.StudentCodes.Any())
        {
            db.StudentCodes.Add(new StudentCode{FixedCode="STU001"});
            db.SaveChanges();
        }
    }

    private static void SeedAdmin(AppDbContext db)
    {
        var admin=db.Users.FirstOrDefault(u=>u.Username=="admin");

        if (admin==null)
        {
            admin=new User
            {
                Username="admin",
                Password=BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName="مدیر سیستم",
                Email="admin@uni.local",
                UserType="Staff",
                IsSuperAdmin=true
            };
            db.Users.Add(admin);
            db.SaveChanges();
        }
        else if (!admin.IsSuperAdmin)
        {
            admin.IsSuperAdmin=true;
            db.SaveChanges();
        }
    }

    private static void BackfillUserPermissions(AppDbContext db)
    {
        var allUsers=db.Users.ToList();
        var permissionsByCode=db.Permissions.ToDictionary(p=>p.Code);

        foreach (var user in allUsers)
        {
            var hasAny=db.UserPermissions.Any(up=>up.UserID==user.UserID);
            if (hasAny)
            {
                continue;
            }

            var defaultCodes=DefaultPermissionsForRole(user.UserType);
            foreach (var code in defaultCodes)
            {
                if (!permissionsByCode.ContainsKey(code))
                {
                    continue;
                }
                db.UserPermissions.Add(new UserPermission
                {
                    UserID=user.UserID,
                    PermissionID=permissionsByCode[code].PermissionID
                });
            }
        }

        db.SaveChanges();
    }

    public static List<int> GetDefaultPermissionIds(AppDbContext db,string role)
    {
        var codes=DefaultPermissionsForRole(role).ToList();
        if (codes.Count==0)
        {
            return new List<int>();
        }
        return db.Permissions
            .Where(p=>codes.Contains(p.Code))
            .Select(p=>p.PermissionID)
            .ToList();
    }

    public static IEnumerable<string> DefaultPermissionsForRole(string role)
    {
        return role switch
        {
            "Staff"=>StaffPermissions,
            "Professor"=>ProfessorPermissions,
            "Student"=>StudentPermissions,
            _=>Array.Empty<string>()
        };
    }

    public static readonly List<Permission> AllPermissions=new()
    {
        new Permission{Code="Users.View",Description="مشاهده لیست کاربران",Category="Users"},
        new Permission{Code="Users.Create",Description="افزودن کاربر",Category="Users"},
        new Permission{Code="Users.Delete",Description="حذف کاربر",Category="Users"},
        new Permission{Code="Users.ManagePermissions",Description="مدیریت اختیارات کاربران",Category="Users"},

        new Permission{Code="Courses.View",Description="مشاهده لیست دروس",Category="Courses"},
        new Permission{Code="Courses.Create",Description="افزودن درس",Category="Courses"},
        new Permission{Code="Courses.Delete",Description="حذف درس",Category="Courses"},

        new Permission{Code="Exams.View",Description="مشاهده امتحان‌ها",Category="Exams"},
        new Permission{Code="Exams.Create",Description="ساخت امتحان",Category="Exams"},
        new Permission{Code="Exams.Delete",Description="حذف امتحان",Category="Exams"},
        new Permission{Code="Exams.Grade",Description="تصحیح امتحان دانشجویان",Category="Exams"},
        new Permission{Code="Exams.GradeBulk",Description="تصحیح گروهی امتحان",Category="Exams"},

        new Permission{Code="Assignments.View",Description="مشاهده تمرین‌های دانشجویان",Category="Assignments"},
        new Permission{Code="Assignments.Grade",Description="نمره‌دهی تمرین",Category="Assignments"},

        new Permission{Code="Grades.Edit",Description="ویرایش نمرات درس",Category="Grades"},
        new Permission{Code="Grades.ViewAll",Description="مشاهده گزارش کلی نمرات",Category="Grades"},

        new Permission{Code="Student.Courses.View",Description="مشاهده دروس من",Category="Student"},
        new Permission{Code="Student.Exams.View",Description="مشاهده امتحانات",Category="Student"},
        new Permission{Code="Student.Exams.Take",Description="شرکت در امتحان",Category="Student"},
        new Permission{Code="Student.Assignments.View",Description="مشاهده تمرین‌های من",Category="Student"},
        new Permission{Code="Student.Assignments.Submit",Description="ارسال تمرین",Category="Student"},
        new Permission{Code="Student.Grades.View",Description="مشاهده نمرات من",Category="Student"}
    };

    public static readonly string[] StaffPermissions=
    {
        "Users.View","Users.Create","Users.Delete","Users.ManagePermissions",
        "Courses.View","Courses.Create","Courses.Delete",
        "Grades.ViewAll"
    };

    public static readonly string[] ProfessorPermissions=
    {
        "Exams.View","Exams.Create","Exams.Delete","Exams.Grade","Exams.GradeBulk",
        "Assignments.View","Assignments.Grade",
        "Grades.Edit"
    };

    public static readonly string[] StudentPermissions=
    {
        "Student.Courses.View",
        "Student.Exams.View","Student.Exams.Take",
        "Student.Assignments.View","Student.Assignments.Submit",
        "Student.Grades.View"
    };
}