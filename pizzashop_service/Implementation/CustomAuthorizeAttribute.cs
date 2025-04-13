using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using pizzashop_service.Interface;

public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _moduleName;
    private readonly string _permissionType;

    public CustomAuthorizeAttribute(string moduleName, string permissionType)
    {
        _moduleName = moduleName;
        _permissionType = permissionType;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }
                                                                                
        var userRole = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userRole))
        {
            context.Result =  new RedirectToActionResult("AccessDenied", "Auth", null);
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        if (permissionService == null)
        {
            context.Result =  new RedirectToActionResult("AccessDenied", "Auth", null);
            return;
        }

        var permissions = await permissionService.GetPermissionsByRoleAsync(userRole);

        bool hasPermission = permissions.Any(p =>
            p.PermissionName == _moduleName && 
            ((_permissionType == "CanView" && p.Canview) ||
            (_permissionType == "CanAddEdit" && p.CanaddEdit) ||
            (_permissionType == "CanDelete" && p.Candelete)));

        Console.WriteLine($"User Role: {userRole}, Module: {_moduleName}, Permission Type: {_permissionType}, Has Permission: {hasPermission}");

        if (!hasPermission)
        {
            Console.WriteLine("Access Denied!");
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
        }
        else
        {
            Console.WriteLine("Access Granted!");
        }
    }
}
