using Microsoft.AspNetCore.Mvc;
using UniversityManagementSystem.Data;
using UniversityManagementSystem.Filters;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers;

public class UsersController:Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db=db;
    }

    [PermissionAuthorize("Users.View")]
    [HttpGet]
    public IActionResult Index()
    {
        var users=_db.Users
            .OrderBy(u=>u.UserType)
            .ThenBy(u=>u.FullName)
            .Select(u=>new UserListItemViewModel
            {
                UserID=u.UserID,
                Username=u.Username,
                FullName=u.FullName,
                Email=u.Email,
                UserType=u.UserType,
                IsSuperAdmin=u.IsSuperAdmin,
                PermissionCount=u.IsSuperAdmin ? _db.Permissions.Count() : u.UserPermissions.Count
            })
            .ToList();

        ViewBag.CanManagePermissions=HttpContext.Session.GetString("IsSuperAdmin")=="true";

        return View(users);
    }

    [PermissionAuthorize("Users.Create")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new UserCreateViewModel());
    }

    [PermissionAuthorize("Users.Create")]
    [HttpPost]
    public IActionResult Create(UserCreateViewModel model)
    {
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) ||
            string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Email) ||
            string.IsNullOrEmpty(model.Role))
        {
            ViewBag.Error="همه فیلدها الزامی است.";
            return View(model);
        }

        if (model.Role!="Student" && model.Role!="Professor" && model.Role!="Staff")
        {
            ViewBag.Error="نقش انتخاب‌شده معتبر نیست.";
            return View(model);
        }

        if (_db.Users.Any(u=>u.Username==model.Username))
        {
            ViewBag.Error="این نام کاربری قبلاً استفاده شده است.";
            return View(model);
        }

        if (_db.Users.Any(u=>u.Email==model.Email))
        {
            ViewBag.Error="این ایمیل قبلاً استفاده شده است.";
            return View(model);
        }

        var user=new User
        {
            Username=model.Username,
            Password=BCrypt.Net.BCrypt.HashPassword(model.Password),
            FullName=model.FullName,
            Email=model.Email,
            UserType=model.Role
        };

        var isSuperAdmin=HttpContext.Session.GetString("IsSuperAdmin")=="true";
        if (model.Role=="Staff" && isSuperAdmin)
        {
            user.IsSuperAdmin=true;
        }

        _db.Users.Add(user);
        _db.SaveChanges();

        var permissionIds=DbInitializer.GetDefaultPermissionIds(_db,model.Role);
        foreach (var pid in permissionIds)
        {
            _db.UserPermissions.Add(new UserPermission
            {
                UserID=user.UserID,
                PermissionID=pid
            });
        }
        _db.SaveChanges();

        return RedirectToAction("Index");
    }

    [PermissionAuthorize("Users.Delete")]
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var currentUserId=HttpContext.Session.GetInt32("UserId");
        if (currentUserId==id)
        {
            return RedirectToAction("Index");
        }

        var user=_db.Users.FirstOrDefault(u=>u.UserID==id);
        if (user==null)
        {
            return RedirectToAction("Index");
        }

        var enrollments=_db.Enrollments.Where(e=>e.StudentID==id).ToList();
        _db.Enrollments.RemoveRange(enrollments);

        var taughtCourses=_db.Courses.Where(c=>c.ProfessorID==id).ToList();
        foreach (var course in taughtCourses)
        {
            course.ProfessorID=null;
        }

        _db.Users.Remove(user);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }

    [PermissionAuthorize("Users.ManagePermissions")]
    [HttpGet]
    public IActionResult Permissions(int id)
    {
        var user=_db.Users.FirstOrDefault(u=>u.UserID==id);
        if (user==null)
        {
            return RedirectToAction("Index");
        }

        var allPermissions=_db.Permissions
            .OrderBy(p=>p.Category)
            .ThenBy(p=>p.Code)
            .ToList();

        var selectedIds=user.IsSuperAdmin
            ? allPermissions.Select(p=>p.PermissionID).ToHashSet()
            : _db.UserPermissions.Where(up=>up.UserID==id).Select(up=>up.PermissionID).ToHashSet();

        var currentUserId=HttpContext.Session.GetInt32("UserId");
        var isSuperAdmin=HttpContext.Session.GetString("IsSuperAdmin")=="true";

        var model=new UserPermissionsViewModel
        {
            UserID=user.UserID,
            Username=user.Username,
            FullName=user.FullName,
            UserType=user.UserType,
            IsSuperAdmin=user.IsSuperAdmin,
            CanEdit=isSuperAdmin
        };

        if (!isSuperAdmin)
        {
            model.InfoMessage="فقط پرسنل اصلی سیستم می‌تواند اختیارات را تغییر دهد. این صفحه فقط برای مشاهده است.";
        }

        if (currentUserId==user.UserID)
        {
            model.CanEdit=false;
            model.InfoMessage="شما نمی‌توانید اختیارات خودتان را تغییر دهید.";
        }

        if (user.IsSuperAdmin)
        {
            model.CanEdit=false;
            model.InfoMessage="این کاربر SuperAdmin است و به‌طور خودکار همه اختیارات را دارد. برای تغییر، ابتدا باید SuperAdmin بودنش را بردارید (از طریق دیتابیس).";
        }

        var categoryLabels=new Dictionary<string,string>
        {
            {"Users","مدیریت کاربران"},
            {"Courses","مدیریت دروس"},
            {"Exams","امتحان‌ها"},
            {"Assignments","تمرین‌ها"},
            {"Grades","نمرات"},
            {"Student","بخش دانشجو"}
        };

        foreach (var group in allPermissions.GroupBy(p=>p.Category))
        {
            var cat=new PermissionCategoryViewModel
            {
                Category=group.Key,
                CategoryLabel=categoryLabels.ContainsKey(group.Key) ? categoryLabels[group.Key] : group.Key
            };

            foreach (var p in group)
            {
                cat.Permissions.Add(new PermissionItemViewModel
                {
                    PermissionID=p.PermissionID,
                    Code=p.Code,
                    Description=p.Description,
                    Category=p.Category,
                    IsSelected=selectedIds.Contains(p.PermissionID)
                });
            }

            model.Categories.Add(cat);
        }

        return View(model);
    }

    [PermissionAuthorize("Users.ManagePermissions")]
    [HttpPost]
    public IActionResult Permissions(int id,List<int>? permissionIds)
    {
        var currentUserId=HttpContext.Session.GetInt32("UserId");
        if (currentUserId==id)
        {
            TempData["Error"]="شما نمی‌توانید اختیارات خودتان را تغییر دهید.";
            return RedirectToAction("Permissions",new{id});
        }

        if (HttpContext.Session.GetString("IsSuperAdmin")!="true")
        {
            TempData["Error"]="فقط پرسنل اصلی می‌تواند اختیارات را تغییر دهد.";
            return RedirectToAction("Permissions",new{id});
        }

        var user=_db.Users.FirstOrDefault(u=>u.UserID==id);
        if (user==null)
        {
            return RedirectToAction("Index");
        }

        if (user.IsSuperAdmin)
        {
            TempData["Error"]="این کاربر SuperAdmin است و اختیاراتش قابل تغییر نیست.";
            return RedirectToAction("Permissions",new{id});
        }

        if (permissionIds==null)
        {
            permissionIds=new List<int>();
        }

        var validIds=_db.Permissions
            .Where(p=>permissionIds.Contains(p.PermissionID))
            .Select(p=>p.PermissionID)
            .ToHashSet();

        var existing=_db.UserPermissions
            .Where(up=>up.UserID==id)
            .ToList();

        var toRemove=existing.Where(up=>!validIds.Contains(up.PermissionID)).ToList();
        _db.UserPermissions.RemoveRange(toRemove);

        var existingIds=existing.Select(up=>up.PermissionID).ToHashSet();
        foreach (var pid in validIds)
        {
            if (existingIds.Contains(pid))
            {
                continue;
            }
            _db.UserPermissions.Add(new UserPermission
            {
                UserID=id,
                PermissionID=pid
            });
        }

        _db.SaveChanges();

        TempData["Success"]="اختیارات کاربر با موفقیت به‌روزرسانی شد. توجه: تغییرات پس از ورود مجدد کاربر اعمال می‌شود.";
        return RedirectToAction("Permissions",new{id});
    }
}