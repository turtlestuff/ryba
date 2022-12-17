using System;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;
using Ryba.Data;

namespace Ryba.Bot.Extensions;

public static class DbExtensions
{
    public static RybaUser GetOrCreateUser(this RybaContext db, Snowflake snowflake)
        => db.GetOrCreateUser(snowflake.ToString());

    public static bool TryGetOrCreateUser(this RybaContext db, ICommandContext context, out RybaUser user, out Snowflake userID)
    {
        if (!context.TryGetUserID(out var u) || u is not Snowflake uID)
        {
            user = default!;
            userID = default;
            return false;
        }
       
        user = db.GetOrCreateUser(uID);
        userID = uID;
        return true;
    }
}
