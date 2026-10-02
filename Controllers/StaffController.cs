using Microsoft.AspNetCore.Mvc;
using UniversityManagementSystem.Filters;

namespace UniversityManagementSystem.Controllers;

public class StaffController:Controller
{
    [PermissionAuthorize]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}