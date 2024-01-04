using Microsoft.EntityFrameworkCore;

namespace Ryba.Data;

public static class DbExtensions
{
    public static async Task<RybaUser> GetOrCreateUserAsync(this RybaContext db, string id)
    {
        if (await db.Users.FirstOrDefaultAsync(u => u.Id == id) is { } user)
        {
            return user;
        }

        user = new RybaUser { Id = id, PortablePins = [], Language = FluentLocalizationService.DefaultLanguage};
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        return user;
    }
}
