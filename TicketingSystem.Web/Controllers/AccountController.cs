using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Web.Services;

namespace TicketingSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserAuthenticationService _authenticationService;

    public AccountController(
        UserAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        ViewBag.ReturnUrl = returnUrl;

        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string username,
        string password,
        string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                string.Empty,
                "Username and password are required.");

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        var user =
            _authenticationService.Authenticate(
                username,
                password);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username or password.");

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Username),

            new(
                ClaimTypes.Name,
                user.Username),

            new(
                ClaimTypes.Role,
                user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(
            "Index",
            "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}