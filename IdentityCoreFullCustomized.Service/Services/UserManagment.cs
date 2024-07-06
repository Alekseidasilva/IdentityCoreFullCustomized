using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Models.Authentication.SignUp;
using IdentityCoreFullCustomized.Service.Models.Authentication.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace IdentityCoreFullCustomized.Service.Services;

public class UserManagment : IUserManagment
{
    #region Variables
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly SignInManager<IdentityUser> _signInManager;

    #endregion

    public UserManagment(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmailService emailService, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _signInManager = signInManager;
    }

    public async Task<ApiResponse<UserCreateResponse>> CreateUserWithTokenAsync(RegisterUser registerUser)
    {
        //Check User Exists
        var userExistes = await _userManager.FindByEmailAsync(registerUser.Email);
        if (userExistes != null)
            return new ApiResponse<UserCreateResponse> { IsSuccess = false, StatusCode = 403, Message = "User already Exists!" };

        //Add the User in the Database
        IdentityUser user = new()
        {
            Email = registerUser.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerUser.UserName,
            TwoFactorEnabled = true
        };
        var result = await _userManager.CreateAsync(user, registerUser.Password);
        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return new ApiResponse<UserCreateResponse> { Response = new UserCreateResponse() { User = user, Token = token }, IsSuccess = true, StatusCode = 201, Message = "User Created!" };

        }
        else
        {
            return new ApiResponse<UserCreateResponse> { IsSuccess = false, StatusCode = 500, Message = "User Failed to Create!" };
        }

    }

    public async Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, IdentityUser user)
    {
        var assignedRole = new List<string>();
        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                    assignedRole.Add(role);
                }

            }
        }
        return new ApiResponse<List<string>> { IsSuccess = true, StatusCode = 200, Message = "Roles has been asssigned", Response = assignedRole };
    }
}