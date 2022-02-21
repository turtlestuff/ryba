using System;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Ryba.Data;

namespace Ryba.Bot.Extensions;

public static class DbExtensions
{
    public static RybaUser GetOrCreateUser(this RybaContext db, Snowflake snowflake)
    {
        if (db.Users.Include(a => a.PortablePins)
            .SingleOrDefault(u => u.Id == snowflake.ToString()) is RybaUser user)
        {
            return user;
        }
        else
        {
            user = new RybaUser { Id = snowflake.ToString(), PortablePins = new() };
            db.Users.Add(user);
            return user;
        }
    }
}
