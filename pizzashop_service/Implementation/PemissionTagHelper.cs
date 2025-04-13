using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

public static class PermissionHelper
{
    public static async Task<RolePermissionViewModel> GetPermissionsAsync(HttpContext context, string requiredPermissionName)
    {
        var userClaims = context.User;
        if (userClaims == null || !userClaims.Identity.IsAuthenticated)
            return new RolePermissionViewModel(); 

        var roleName = userClaims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleName))
            return new RolePermissionViewModel();

        var userService = context.RequestServices.GetService<IUserService>();
        if (userService == null)
            return new RolePermissionViewModel();

        var permissions = await userService.GetPermissionsByRoleAsync(roleName);

        var permission = permissions.FirstOrDefault(p => p.PermissionName == requiredPermissionName);
        if (permission == null)
            return new RolePermissionViewModel();

        return new RolePermissionViewModel
        {
            Canview = permission.Canview,
            CanaddEdit = permission.CanaddEdit,
            Candelete = permission.Candelete
        };
    }
}

