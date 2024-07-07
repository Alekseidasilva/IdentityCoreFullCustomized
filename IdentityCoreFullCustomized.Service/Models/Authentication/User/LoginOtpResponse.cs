using IdentityCoreFullCustomized.Data.Models;

namespace IdentityCoreFullCustomized.Service.Models.Authentication.User;

public class LoginOtpResponse
{
    public string Token { get; set; } = null!;
    public bool IsTwoFactoryEnabled { get; set; }
    public ApplicationUser User { get; set; } = null!;
}