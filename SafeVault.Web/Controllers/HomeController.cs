using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace SafeVault.Web.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => View();
    [Authorize] public IActionResult Profile() => View();
    public IActionResult Error() => View();
}
