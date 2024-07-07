using IdentityCoreFullCustomized.Data.Models;

namespace IdentityCoreFullCustomized.Service.Models.Authentication.User;

public class UserCreateResponse
{
    public string Token { get; set; }
    public ApplicationUser User { get; set; }
}