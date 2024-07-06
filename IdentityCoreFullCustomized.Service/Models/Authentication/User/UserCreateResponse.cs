using Microsoft.AspNetCore.Identity;

namespace IdentityCoreFullCustomized.Service.Models.Authentication.User;

public class UserCreateResponse
{
    public string Token { get; set; }
    public IdentityUser User { get; set; }
}