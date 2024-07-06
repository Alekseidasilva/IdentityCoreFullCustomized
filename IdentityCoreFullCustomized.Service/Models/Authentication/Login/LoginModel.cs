using System.ComponentModel.DataAnnotations;

namespace IdentityCoreFullCustomized.Service.Models.Authentication.Login;

public class LoginModel
{
    [Required(ErrorMessage = "User name is Required")]
    public string? UserName { get; set; }
    [Required(ErrorMessage = "Password is Required")]
    public string? Password { get; set; }
}