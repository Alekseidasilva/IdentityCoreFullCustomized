using IdentityCoreFullCustomized.Models;
using IdentityCoreFullCustomized.Models.Authentication.SignUp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCoreFullCustomized.Controllers;
[Route("api/controller")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
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
        var result = await _userManager.CreateAsync(user, registerUser.Password);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created,
                new Response { Status = "Success", Message = "User Created Sucessfully!" })
            : StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "User Failed to Create!" });
        //Assign a Role

    }
}