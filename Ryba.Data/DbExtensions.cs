using Microsoft.EntityFrameworkCore;

namespace Ryba.Data;

public static class DbExtensions
{
    public static RybaUser GetOrCreateUser(this RybaContext db, string id)
    {
        if (db.Users.Include(a => a.PortablePins)
            .SingleOrDefault(u => u.Id == id) is RybaUser user)
        {
            return user;
        }
        else
        {
            user = new RybaUser { Id = id, PortablePins = new(), Language = FluentLocalizationService.DefaultLanguage};
            db.Users.Add(user);
            return user;
        }
    }
}
