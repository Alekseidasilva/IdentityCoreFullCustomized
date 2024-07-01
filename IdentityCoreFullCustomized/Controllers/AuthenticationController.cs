using Microsoft.AspNetCore.Mvc;

namespace IdentityCoreFullCustomized.Controllers;

public class AuthenticationController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}