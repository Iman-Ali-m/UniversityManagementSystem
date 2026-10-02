using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UniversityManagementSystem.Filters;

public class PermissionAuthorizeAttribute:Attribute,IAuthorizationFilter
{
    private readonly string[] _permissions;

    public PermissionAuthorizeAttribute(params string[] permissions)
    {
        _permissions=permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId=context.HttpContext.Session.GetInt32("UserId");
        if (userId==null)
        {
            context.Result=new RedirectToActionResult("Login","Account",null);
            return;
        }

        if (context.HttpContext.Session.GetString("IsSuperAdmin")=="true")
        {
            return;
        }

        if (_permissions.Length==0)
        {
            return;
        }

        var stored=context.HttpContext.Session.GetString("Permissions") ?? string.Empty;
        if (string.IsNullOrEmpty(stored))
        {
            context.Result=new RedirectToActionResult("Forbidden","Home",null);
            return;
        }

        var userPermissions=stored.Split(",",StringSplitOptions.RemoveEmptyEntries);
        var hasAccess=_permissions.Any(p=>userPermissions.Contains(p));

        if (!hasAccess)
        {
            context.Result=new RedirectToActionResult("Forbidden","Home",null);
        }
    }
}