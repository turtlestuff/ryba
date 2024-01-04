using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Ryba.Web.Data;
[Route("[controller]/[action]")] // Microsoft.AspNetCore.Mvc.Route
public class AccountController(IDataProtectionProvider provider) : ControllerBase
{
    public IDataProtectionProvider Provider { get; } = provider;

    [HttpGet]
    public IActionResult Login(string returnUrl = "/") =>
        Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, 
            DiscordAuthenticationDefaults.AuthenticationScheme);

    [HttpGet]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(returnUrl);
    }
}
