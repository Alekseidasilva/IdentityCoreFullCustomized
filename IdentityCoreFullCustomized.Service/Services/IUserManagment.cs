using IdentityCoreFullCustomized.Service.Models;
using IdentityCoreFullCustomized.Service.Models.Authentication.SignUp;

namespace IdentityCoreFullCustomized.Service.Services;

public interface IUserManagment
{
    /// <summary>
    /// Brief Description of what the method does
    /// </summary>
    /// <param name="registerUser">Description of the parameter</param>
    /// <returns>description of the return value</returns>
    Task<ApiResponse<string>> CreateUserWithTokenAsync(RegisterUser registerUser);
}