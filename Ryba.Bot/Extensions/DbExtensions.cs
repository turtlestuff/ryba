using System;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Ryba.Data;

namespace Ryba.Bot.Extensions;

public static class DbExtensions
{
    public static RybaUser GetOrCreateUser(this RybaContext db, Snowflake snowflake)
        => db.GetOrCreateUser(snowflake.ToString());
}
