using Microsoft.AspNetCore.Mvc;
using UniversityManagementSystem.Data;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers;

public class AccountController:Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db=db;
    }

    [HttpGet]
    public IActionResult Login()
    {
        var userId=HttpContext.Session.GetInt32("UserId");
        if (userId!=null)
        {
            return RedirectToDashboard(HttpContext.Session.GetString("UserRole"));
        }
        return View(new LoginViewModel());
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Role))
        {
            ViewBag.Error="همه فیلدها الزامی است.";
            return View(model);
        }

        var user=_db.Users.FirstOrDefault(u=>u.Username==model.Username && u.UserType==model.Role);
        if (user==null || !BCrypt.Net.BCrypt.Verify(model.Password,user.Password))
        {
            ViewBag.Error="نام کاربری یا رمز عبور اشتباه است.";
            return View(model);
        }

        SetSession(user);
        return RedirectToDashboard(user.UserType);
    }

    [HttpGet]
    public IActionResult Register(string? role)
    {
        if (string.IsNullOrEmpty(role))
        {
            role="Student";
        }
        return View(new RegisterViewModel{Role=role});
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) ||
            string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Email) ||
            string.IsNullOrEmpty(model.Role))
        {
            ViewBag.Error="همه فیلدها الزامی است.";
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

        if (string.IsNullOrEmpty(model.FixedCode))
        {
            ViewBag.Error="کد ثابت الزامی است.";
            return View(model);
        }

        bool codeValid=model.Role switch
        {
            "Staff"=>_db.StaffCodes.Any(c=>c.FixedCode==model.FixedCode),
            "Professor"=>_db.ProfessorCodes.Any(c=>c.FixedCode==model.FixedCode),
            "Student"=>_db.StudentCodes.Any(c=>c.FixedCode==model.FixedCode),
            _=>false
        };

        if (!codeValid)
        {
            ViewBag.Error="کد ثابت نامعتبر است.";
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

        if (model.Role=="Staff")
        {
            user.IsSuperAdmin=true;
        }

        _db.SaveChanges();

        TempData["Success"]="ثبت‌نام با موفقیت انجام شد.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private void SetSession(User user)
    {
        HttpContext.Session.SetInt32("UserId",user.UserID);
        HttpContext.Session.SetString("UserRole",user.UserType);
        HttpContext.Session.SetString("Email",user.Email);
        HttpContext.Session.SetString("FullName",user.FullName);
        HttpContext.Session.SetString("IsSuperAdmin",user.IsSuperAdmin ? "true" : "false");

        List<string> permissionCodes;

        if (user.IsSuperAdmin)
        {
            permissionCodes=_db.Permissions.Select(p=>p.Code).ToList();
        }
        else
        {
            permissionCodes=_db.UserPermissions
                .Where(up=>up.UserID==user.UserID)
                .Select(up=>up.Permission!.Code)
                .ToList();
        }

        HttpContext.Session.SetString("Permissions",string.Join(",",permissionCodes));
    }

    private IActionResult RedirectToDashboard(string? role)
    {
        return role switch
        {
            "Staff"=>RedirectToAction("Index","Staff"),
            "Professor"=>RedirectToAction("Index","Professor"),
            "Student"=>RedirectToAction("Index","Student"),
            _=>RedirectToAction("Login","Account")
        };
    }
}