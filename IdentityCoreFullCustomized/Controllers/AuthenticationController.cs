﻿using IdentityCoreFullCustomized.Api.Models;
using IdentityCoreFullCustomized.Api.Models.Authentication.Login;
using IdentityCoreFullCustomized.Api.Models.Authentication.SignUp;
using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace IdentityCoreFullCustomized.Api.Controllers;
[Route("api/controller")]
public class AuthenticationController : ControllerBase
{
    #region Variables
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly SignInManager<IdentityUser> _signInManager;

    #endregion
    #region Builders
    public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmailService emailService, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _signInManager = signInManager;
    }
    #endregion
    #region Methods
    #region Register
    [HttpPost(nameof(Register))]
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
            UserName = registerUser.UserName,
            TwoFactorEnabled = true
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


            //Add Tojen to Verify the Email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email });
            var message = new Message(new string[] { user.Email! }, "Confirmation Email Link", confirmLink);
            //  _emailService.SendEmail(message);


            return StatusCode(StatusCodes.Status200OK,
                new Response { Status = "Success", Message = $"User Created & Email Confirmation was sent to {user.Email} Successfully!" });
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "This Role  does not exists!" });
        }
    }


    #endregion
    #region ConfirmEmail
    [HttpGet("ConfirmeEmail")]
    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = "Email Verify Successfully" });
            }
        }
        return StatusCode(StatusCodes.Status500InternalServerError,
            new Response { Status = "Error", Message = "This user doesnt exist" });
    }


    #endregion
    #region Login
    [HttpPost]
    [Route(nameof(Login))]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        //Checking the user
        var user = await _userManager.FindByNameAsync(loginModel.UserName);
        //To Factor Auth
        if (user.TwoFactorEnabled)
        {
            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            var message = new Message(new string[] { user.Email! }, "OTP Confirmation", token);

            //Todo: Send Email
            //Console.WriteLine($"OTP Code: {token}");
            //  _emailService.SendEmail(message);








            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = $"We have sent an OTP to your Email {user.Email}" });
        }
        //Checking Password
        if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
        {
            //ClaimList Creation
            var authClaims = new List<Claim>
           {
               new Claim(ClaimTypes.Name, user.UserName),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
           };
            var userRoles = await _userManager.GetRolesAsync(user);
            //We add Roles to the List
            foreach (var role in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));


            //Generate the Token with the claims
            var jwtToken = GetToken(authClaims);

            //returnn the token
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                expiration = jwtToken.ValidTo
            });
        }
        return Unauthorized();

    }
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:ValidIssuer"],
            audience: _configuration["Jwt:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256));
        return token;
    }
    #endregion
    #region OTP
    [HttpPost]
    [Route("login-2FA")]
    public async Task<IActionResult> LoginWithOTP(string code, string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        //Todo: Terminar Authentication with two factor
        var signIn = await _signInManager.TwoFactorSignInAsync("Email", code, false, false);
        if (signIn.Succeeded)
        {
            if (user != null)
            {
                //ClaimList Creation
                var authClaims = new List<Claim>
                {
                    new (ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var userRoles = await _userManager.GetRolesAsync(user);

                //We add Roles to the List
                foreach (var role in userRoles)
                    authClaims.Add(new Claim(ClaimTypes.Role, role));

                //Generate the Token with the claims
                var jwtToken = GetToken(authClaims);

                //returnn the token
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
            }
        }
        return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Invalid Code" });
    }


    #endregion
    #endregion
}