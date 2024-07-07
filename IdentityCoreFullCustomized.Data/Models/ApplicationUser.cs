using Microsoft.AspNetCore.Identity;

namespace IdentityCoreFullCustomized.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiry { get; set; }
}