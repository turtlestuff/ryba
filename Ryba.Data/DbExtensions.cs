using System;
using Microsoft.EntityFrameworkCore;
using Ryba.Data;

namespace Ryba.Bot.Extensions;

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
            user = new RybaUser { Id = id, PortablePins = new() };
            db.Users.Add(user);
            return user;
        }
    }
}
