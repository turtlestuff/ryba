using Ryba.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Ryba.Web.Extensions;

static class DbExtensions
{
    public static Task<RybaUser> GetOrCreateUserAsync(this RybaContext db, ClaimsPrincipal user) =>
        db.GetOrCreateUserAsync(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
