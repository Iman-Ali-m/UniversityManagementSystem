using Microsoft.AspNetCore.Mvc;

namespace UniversityManagementSystem.Controllers;

public class HomeController:Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Login","Account");
    }

    public IActionResult Error()
    {
        return View();
    }

    public IActionResult NotFoundPage()
    {
        Response.StatusCode=404;
        return View("NotFound");
    }

    public IActionResult Forbidden()
    {
        Response.StatusCode=403;
        return View();
    }
}