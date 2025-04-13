using Microsoft.AspNetCore.Mvc;

using pizzashop_repository.ViewModels;

using pizzashop_service.Interface;

namespace pizzashop.Controllers;

public class AuthController : Controller
{

    private readonly IUserService _useService;
    private readonly IEmailSender _emailSender;

    private readonly IJwtService _jwtService;

    public AuthController(IUserService userService, IEmailSender emailSender, IJwtService jwtService)
    {
        _useService = userService;
        _emailSender = emailSender;
        _jwtService = jwtService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        string? req_cookie = Request.Cookies["UserEmail"];
        if (!string.IsNullOrEmpty(req_cookie))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user =  _useService.AuthenicateUser(model.Email, model.Password);

            if (user == null)
            {
                // ModelState.AddModelError("", "Invalid Email or Password");
                TempData["error"] =  "Invalid Email or Password";
                return View(model);
            }

            var coockieopt = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            if (model.RememberMe)
            {
                coockieopt.Expires = DateTime.UtcNow.AddDays(30);
                Response.Cookies.Append("UserEmail", user.Email, coockieopt);
            }

            Response.Cookies.Append("Username", user.Username, coockieopt);
            Response.Cookies.Append("ProfileImgPath", string.IsNullOrEmpty(user.Profileimagepath) ? "/images/Default_pfp.svg.png" : user.Profileimagepath, coockieopt);

            string token = await _useService.GenerateJwttoken(user.Email, user.Roleid);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddHours(24)
            });

            TempData["success"] = "Login Successful";

            return RedirectToAction("Index", "Dashboard");

        }

        return View(model);
    }

    [HttpGet]

    public IActionResult ForgotPassword(string email)

    {

        if (!string.IsNullOrEmpty(email))

        {

            ViewData["Email"] = email;

        }

        else

        {

            ViewData["Email"] = "";

        }

        return View();

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotViewModel objUser)
    {
        if (!ModelState.IsValid)
        {
            return View(objUser);
        }

        var user = _useService.GetUserByEmail(objUser.Email);

        if (user == null)
        {
            TempData["error"] = "User with this email does not exits";
            return View(objUser);
        }

        string resetToken = _useService.GeneratePasswordResetToken(user.Email);




        string filePath = @"C:/Users/pci100/Desktop/3tiertryerror/pizzashop/Template/EmailTemplate.html";
        string emailBody = System.IO.File.ReadAllText(filePath);

        var resetLink = Url.Action("ResetPassword", "Auth", new { token = resetToken }, Request.Scheme);
        emailBody = emailBody.Replace("{ResetLink}", resetLink);

        string subject = "Reset Password";
        _emailSender.SendEmailAsync(objUser.Email, subject, emailBody);

        TempData["success"] = "Password reset instructions have been sent to your email.";


        return View(objUser);
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {   
        if(string.IsNullOrEmpty(token))
        {
            TempData["error"]="invalid reset link";
            return RedirectToAction("ForgotPassword");
        }

        var model = new ResetPasswordViewModel { Token = token};
        return View(model);
    }

    [HttpPost]
   
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_useService.ResetPassword(model.Token, model.NewPassword, model.ConfirmPassword, out string message))
        {
            TempData["success"] = "Password Successfully Reset";
            return RedirectToAction("Login", "Auth");
        }

        ModelState.AddModelError(string.Empty, message);
        return View(model);


    }

    [CustomAuthorize("RoleAndPermission", "CanView")]
    [HttpGet]
    public IActionResult Roles()
    {   
        ViewBag.ActiveNav = "Role";
        return View();
    }

    [CustomAuthorize("RoleAndPermission", "CanView")]
    public async Task<IActionResult> Permissions(string role)
    {
        ViewBag.SelectedRole = role;
        var permissions = await _useService.GetPermissionsByRoleAsync(role);
        return View(permissions);
    }


    [HttpPost]
    public async Task<IActionResult> UpdatePermissions([FromBody] List<RolePermissionViewModel> updatedPermissions)
    {
        if (updatedPermissions == null || !updatedPermissions.Any())
        {
            return Json(new { success = false, message = "No permissions received." });
        }

        Console.WriteLine("Received Data:");
        foreach (var perm in updatedPermissions)
        {
            Console.WriteLine($"RoleId: {perm.Roleid}, PermissionId: {perm.Permissionid}, CanView: {perm.Canview}, CanEdit: {perm.CanaddEdit}, CanDelete: {perm.Candelete}");
        }

        var result = await _useService.UpdateRolePermissionsAsync(updatedPermissions);

        return Json(new { success = result });
    }


    public IActionResult AccessDenied()
    {
        return View();
    }


}
