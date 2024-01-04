using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ryba.Data;
using System.Security.Claims;

namespace Ryba.Web.Data;
[Route("[controller]/[action]")]
public class LanguageController : Controller
{
    readonly IDbContextFactory<RybaContext> _rbCtxFac;

    public LanguageController(IDbContextFactory<RybaContext> r, AuthenticationStateProvider a)
    {
        _rbCtxFac = r;
    }

    [Authorize]
    public Task<IActionResult> Set(string? culture, string redirectUri)
    {
        if (culture is not null)
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture, culture)));
        }

        return Task.FromResult<IActionResult>(LocalRedirect(redirectUri));
    }
}
