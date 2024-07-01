using IdentityCoreFullCustomized.Api.Models;
using IdentityCoreFullCustomized.Api.Models.Authentication.SignUp;
using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCoreFullCustomized.Api.Controllers;
[Route("api/controller")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
    }
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, string role)
    {
        //Check User Exists
        var userExistes = await _userManager.FindByEmailAsync(registerUser.Email);
        if (userExistes != null)
            return StatusCode(StatusCodes.Status403Forbidden,
                new Response { Status = "Error", Message = "User already Exists!" });

        //Add the User in the Database
        IdentityUser user = new()
        {
            Email = registerUser.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerUser.Email
        };
        if (await _roleManager.RoleExistsAsync(role))
        {
            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User Failed to Create!" });
            }
            //Add Role to the User
            await _userManager.AddToRoleAsync(user, role);
            return StatusCode(StatusCodes.Status200OK,
                new Response { Status = "Success", Message = "User Created Successfully!" });
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "This Role  does not exists!" });
        }

    }

    [HttpGet]
    public IActionResult TestEmail()
    {
        var message = new Message(new string[] { "alekseidasilva@hotmail.com" }, "Testing..", "<h1>Confirme Email </h1>");

        _emailService.SendEmail(message);
        return StatusCode(StatusCodes.Status500InternalServerError,
            new Response { Status = "Error", Message = "Email Send Successfully" });
    }
}