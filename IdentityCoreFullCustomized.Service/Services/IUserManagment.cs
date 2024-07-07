using IdentityCoreFullCustomized.Data.Models;
using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Models.Authentication.Login;
using IdentityCoreFullCustomized.Service.Models.Authentication.SignUp;
using IdentityCoreFullCustomized.Service.Models.Authentication.User;

namespace IdentityCoreFullCustomized.Service.Services;

public interface IUserManagment
{
    /// <summary>
    /// Brief Description of what the method does
    /// </summary>
    /// <param name="registerUser">Description of the parameter</param>
    /// <returns>description of the return value</returns>
    Task<ApiResponse<UserCreateResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
    Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, ApplicationUser user);
    Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel);

}