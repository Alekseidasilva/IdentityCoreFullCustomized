using IdentityCoreFullCustomized.Api.Models;
using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Models.Authentication.Login;
using IdentityCoreFullCustomized.Service.Models.Authentication.SignUp;
using IdentityCoreFullCustomized.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
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
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    private readonly IUserManagment _user;
    private readonly IConfiguration _configuration;


    #endregion
    #region Builders
    public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmailService emailService, SignInManager<IdentityUser> signInManager, IUserManagment userManagment)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _signInManager = signInManager;
        _user = userManagment;
    }
    #endregion
    #region Methods
    #region Register
    [HttpPost(nameof(Register))]
    public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
    {
        var tokenResponse = await _user.CreateUserWithTokenAsync(registerUser);
        if (tokenResponse.IsSuccess)
        {
            await _user.AssignRoleToUserAsync(registerUser.Roles!, tokenResponse.Response!.User);
            var confirmLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { tokenResponse.Response.Token, email = registerUser.Email }, Request.Scheme);
            var message = new Message(new string[] { registerUser.Email! }, "Confirmation Email Link", confirmLink);
            //  _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status201Created,
                new Response { Status = "Success", Message = "Email verifield Successfully" });
        }


        return StatusCode(StatusCodes.Status201Created,
            new Response { Message = tokenResponse.Message, IsSuccess = false });

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
        var loginOtpResponse = await _user.GetOtpByLoginAsync(loginModel);
        if (loginOtpResponse.Response is not null)
        {
            //Checking the user
            var user = loginOtpResponse.Response!.User;
            //To Factor Auth
            if (user.TwoFactorEnabled)
            {
                var token = loginOtpResponse.Response.Token;
                // var message = new Message(new string[] { user.Email! }, "OTP Confirmation",token);
                //_emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK, new Response { IsSuccess = loginOtpResponse.IsSuccess, Status = "Success", Message = $"We have sent an OTP to your Email {user.Email}" });
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
    #region Reset Password
    [HttpPost("ForgetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgetPassword([Required] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var forgotPasswordlink = Url.Action(nameof(ResetPassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
            //var message = new Message(new string[] { user.Email! }, "Forgot Password Link", forgotPasswordlink);
            //  _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = $"Password Change request is sent on email {user.Email}. Please your email and click the link " });

        }
        return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = $"could not send email, please try again!" });

    }

    [HttpGet("reset-password")]
    public async Task<IActionResult> ResetPassword(string token, string email)
    {
        var model = new ResetPassword { Token = token, Email = email };
        return Ok(new
        {
            model
        });
    }
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> resetpassword(ResetPassword resetPassword)
    {
        var user = await _userManager.FindByEmailAsync(resetPassword.Email);
        if (user != null)
        {
            var resetPasswordResult =
                await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!resetPasswordResult.Succeeded)
            {
                foreach (var error in resetPasswordResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return Ok(ModelState);
            }
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = $"Password Change request is sent on email {user.Email}. Please your email and click the link " });

        }
        return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = $"could not send email, please try again!" });

    }
    #endregion
    #endregion
}