using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Web.Models;
using SafeVault.Web.Services;

namespace SafeVault.Web.Controllers;
public class AccountController : Controller
{
    private readonly AuthService _auth;
    public AccountController(AuthService auth) => _auth = auth;
    [HttpGet] public IActionResult Register() => View(new RegisterForm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterForm input)
    {
        if (!ModelState.IsValid) return View(input);
        if (!await _auth.RegisterAsync(input))
        {
            ModelState.AddModelError("", "Username or email is already in use.");
            return View(input);
        }
        return RedirectToAction(nameof(Login));
    }
    [HttpGet] public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginForm());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginForm input, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(input);
        var user = await _auth.AuthenticateAsync(input.Username, input.Password);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(input);
        }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    [HttpGet] public IActionResult Denied() => View();
}
