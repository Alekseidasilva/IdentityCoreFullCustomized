using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Models.Authentication.SignUp;
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

    public async Task<ApiResponse<string>> CreateUserWithTokenAsync(RegisterUser registerUser)
    {
        //Check User Exists
        var userExistes = await _userManager.FindByEmailAsync(registerUser.Email);
        if (userExistes != null)
            return new ApiResponse<string> { IsSuccess = false, StatusCode = 403, Message = "User already Exists!" };

        //Add the User in the Database
        IdentityUser user = new()
        {
            Email = registerUser.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerUser.UserName,
            TwoFactorEnabled = true
        };
        if (await _roleManager.RoleExistsAsync(registerUser.Role))
        {
            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if (!result.Succeeded)
                return new ApiResponse<string> { IsSuccess = false, StatusCode = 500, Message = "User Failed to Create!" };
            //Add Role to the User
            await _userManager.AddToRoleAsync(user, registerUser.Role);
            //Add Tojen to Verify the Email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return new ApiResponse<string> { IsSuccess = true, StatusCode = 201, Message = $"User Created Successfully!", Response = token };
        }
        else
            return new ApiResponse<string> { IsSuccess = false, StatusCode = 500, Message = "Provided role  does not exists in the database!" };


    }
}