using Microsoft.AspNetCore.Identity;

namespace IdentityCoreFullCustomized.Service.Models.Authentication.User;

public class LoginOtpResponse
{
    public string Token { get; set; } = null!;
    public bool IsTwoFactoryEnabled { get; set; }
    public IdentityUser User { get; set; } = null!;
}