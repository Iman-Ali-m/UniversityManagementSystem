namespace UniversityManagementSystem.Filters;

public static class SessionExtensions
{
    public static bool HasPermission(this HttpContext context,string permission)
    {
        if (context.Session.GetString("IsSuperAdmin")=="true")
        {
            return true;
        }

        var stored=context.Session.GetString("Permissions") ?? string.Empty;
        if (string.IsNullOrEmpty(stored))
        {
            return false;
        }

        var list=stored.Split(",",StringSplitOptions.RemoveEmptyEntries);
        return list.Contains(permission);
    }

    public static bool HasAnyPermission(this HttpContext context,params string[] permissions)
    {
        if (context.Session.GetString("IsSuperAdmin")=="true")
        {
            return true;
        }

        var stored=context.Session.GetString("Permissions") ?? string.Empty;
        if (string.IsNullOrEmpty(stored))
        {
            return false;
        }

        var list=stored.Split(",",StringSplitOptions.RemoveEmptyEntries);
        return permissions.Any(p=>list.Contains(p));
    }
}