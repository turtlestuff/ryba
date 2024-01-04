using System;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;
using Ryba.Data;

namespace Ryba.Bot.Extensions;

static class DbExtensions
{
    public static Task<RybaUser> GetOrCreateUserAsync(this RybaContext db, Snowflake snowflake)
        => db.GetOrCreateUserAsync(snowflake.ToString());
}
