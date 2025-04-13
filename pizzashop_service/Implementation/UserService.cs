using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    private readonly IConfiguration _configuration;

    private readonly PizzaShopDbContext _context;
    public UserService(IUserRepository userRepository, IJwtService jwtService, PizzaShopDbContext context, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _context = context;
        _configuration = configuration;

    }

    public User? GetUserByEmail(string email)
    {
        return _userRepository.GetUserByEmail(email);
    }

    public User? GetUserByUsername(string username)
    {
        return _userRepository.GetUserByUsername(username);
    }
    public User? AuthenicateUser(string email, string password)
    {
        User? user = _userRepository.GetUserByEmail(email);

        if (user == null)
        {
            return null;
        }
        if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$") && !user.Password.StartsWith("$2y$"))
        {
            //  First-time login Hash and update the database
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = hashedPassword;
            _userRepository.UpdateUser(user);
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }
        return user;
    }

    public async Task<string> GenerateJwttoken(string email, int roleId)
    {
        return await _jwtService.GenerateJwtToken(email, roleId);
    }

    public string GeneratePasswordResetToken(string email)
    {
        var user = _userRepository.GetUserByEmail(email);
        if (user == null) return null;

        user.PasswordResetToken = Guid.NewGuid().ToString();

        user.Resettokenexpiry = DateTime.UtcNow.AddHours(1);


        _userRepository.UpdateUser(user);

        return user.PasswordResetToken;
    }


    public bool ResetPassword(string token, string newPassword, string confirmPassword, out string message)
    {
        if (newPassword != confirmPassword)
        {
            message = "The new password and confirmation password do not match.";
            return false;
        }

        var user = _userRepository.GetUserByResetToken(token);

        if (user == null || user.Resettokenexpiry < DateTime.UtcNow)
        {
            message = "Invalid or expired reset token.";
            return false;
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);


        user.PasswordResetToken = null;
        user.Resettokenexpiry = null;

        _userRepository.UpdateUser(user);

        message = "Password has been successfully updated.";
        return true;
    }


    public string ExtractEmailFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return string.Empty;

        var handler = new JwtSecurityTokenHandler();
        var AuthToken = handler.ReadJwtToken(token);
        return AuthToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public UserTableViewModel? GetUserProfile(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var user = _userRepository.GetUserByEmailAndRole(email);
        if (user == null)
            return null;

        return new UserTableViewModel
        {
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            Username = user.Username,
            Email = user.Email,
            Rolename = user.Role.Rolename,
            CountryId = user.Countryid,
            StateId = user.Stateid,
            CityId = user.Cityid,
            Phone = user.Phone,
            Address = user.Address,
            Zipcode = user.Zipcode,
            ProfileImagePath = user.Profileimagepath

        };
    }

    public bool UpdateUserProfile(string email, UserTableViewModel model)
    {
        var user = _userRepository.GetUserByEmail(email);
        if (user == null) return false;

        user.Firstname = model.Firstname;
        user.Lastname = model.Lastname;
        user.Username = model.Username;
        user.Phone = model.Phone;
        user.Countryid = model.CountryId;
        user.Stateid = model.StateId;
        user.Cityid = model.CityId;
        user.Address = model.Address;
        user.Zipcode = model.Zipcode;


        if (model.ProfileImage != null)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);


            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                model.ProfileImage.CopyTo(fileStream);
            }


            if (!string.IsNullOrEmpty(user.Profileimagepath))
            {
                string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Profileimagepath.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            user.Profileimagepath = "/images/users/" + uniqueFileName;
        }

        model.ProfileImagePath = user.Profileimagepath;

        return _userRepository.UpdateUser(user);
    }

    public string ChangePassword(string email, ChangePasswordViewModel model)
    {
        var user = _userRepository.GetUserByEmail(email);

        if (user == null)
        {
            return "UserNotFound";
        }

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
        {
            return "IncorrectPassword";
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        _userRepository.UpdateUser(user);

        return "Success";
    }
    public async Task<PagedResult<UserTableViewModel>> GetUsersAsync(int pageNumber, int pageSize, string sortBy, string sortOrder, string searchTerm = "")
    {
        return await _userRepository.GetUsersAsync(pageNumber, pageSize, sortBy, sortOrder, searchTerm);
    }

    public bool DeleteUser(int id)
    {
        var user = _userRepository.GetUserById(id);
        if (user == null)
        {
            return false; // User not found
        }

        _userRepository.SoftDeleteUser(user);
        return true;
    }

    public List<Role> GetRoles()
    {
        return _userRepository.GetRoles();
    }

    public async Task<bool> AddUser(AddUserViewModel model)
    {
        var role = _userRepository.GetRoleById(model.RoleId);
        if (role == null)
        {
            return false; // Role does not exist
        }

        var user = new User
        {
            Firstname = model.Firstname,
            Lastname = model.Lastname,
            Email = model.Email,
            Username = model.Username,
            Phone = model.Phone,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Roleid = model.RoleId,
            Profileimagepath = model.ProfileImagePath,
            Countryid = model.CountryId,
            Stateid = model.StateId,
            Cityid = model.CityId,
            Address = model.Address,
            Zipcode = model.Zipcode,
            Createdby = role.Rolename

        };


        if (model.ProfileImage != null)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");


            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(fileStream);
            }

            user.Profileimagepath = "/images/users/" + uniqueFileName;
        }

        _userRepository.AddUser(user);

        return true;
    }


    public EditUserViewModel GetUserForEdit(int id)
    {
        var user = _userRepository.GetUserById(id);

        if (user == null) return null;

        return new EditUserViewModel
        {
            id = user.Id,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            Email = user.Email,
            Username = user.Username,
            Phone = user.Phone,
            Status = user.Status,
            RoleId = user.Roleid,
            CountryId = user.Countryid,
            StateId = user.Stateid,
            CityId = user.Cityid,
            Address = user.Address,
            Zipcode = user.Zipcode,
            ProfileImagePath = user.Profileimagepath
        };
    }

    public async Task<bool> EditUser(int id, EditUserViewModel model)
    {
        var user = _userRepository.GetUserById(id);
        if (user == null) return false;

        // Check if any changes were made before updating
        bool hasChanges =
            user.Firstname != model.Firstname ||
            user.Lastname != model.Lastname ||
            user.Email != model.Email ||
            user.Username != model.Username ||
            user.Phone != model.Phone ||
            user.Status != model.Status ||
            user.Roleid != model.RoleId ||
            user.Countryid != model.CountryId ||
            user.Stateid != model.StateId ||
            user.Cityid != model.CityId ||
            user.Address != model.Address ||
            user.Zipcode != model.Zipcode ||
            (model.ProfileImage != null && model.ProfileImage.Length > 0);

        if (!hasChanges)
        {
            return false; // No changes detected, return without updating
        }

        user.Firstname = model.Firstname;
        user.Lastname = model.Lastname;
        user.Email = model.Email;
        user.Username = model.Username;
        user.Phone = model.Phone;
        user.Status = model.Status;
        user.Roleid = model.RoleId;
        user.Countryid = model.CountryId;
        user.Stateid = model.StateId;
        user.Cityid = model.CityId;
        user.Address = model.Address;
        user.Zipcode = model.Zipcode;

        // Check if a new profile image is uploaded
        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(fileStream);
            }

            // Delete old image if it exists
            if (!string.IsNullOrEmpty(user.Profileimagepath))
            {
                string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Profileimagepath.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Update the new image path
            user.Profileimagepath = "/images/users/" + uniqueFileName;
        }

        _userRepository.UpdateUser(user);
        return true;
    }

    public async Task<List<RolePermissionViewModel>> GetPermissionsByRoleAsync(string roleName)
    {
        return await _userRepository.GetPermissionsByRoleAsync(roleName);
    }

    public async Task<bool> UpdateRolePermissionsAsync(List<RolePermissionViewModel> permissions)
    {
        return await _userRepository.UpdateRolePermissionsAsync(permissions);
    }
}
