using Ryba.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Ryba.Web.Extensions;

public static class DbExtensions
{
    public static RybaUser GetOrCreateUser(this RybaContext db, ClaimsPrincipal user) =>
        db.GetOrCreateUser(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
