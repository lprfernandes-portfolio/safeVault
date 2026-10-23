using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Web.Data;
namespace SafeVault.Web.Controllers;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserRepository _repository;
    public AdminController(UserRepository repository) => _repository = repository;
    public async Task<IActionResult> Index() => View(await _repository.ListAsync());
}
