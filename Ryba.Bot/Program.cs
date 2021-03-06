using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Remora.Commands.Extensions;
using Remora.Discord.API;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Interactivity.Extensions;
using Remora.Discord.Pagination.Extensions;
using Remora.Rest.Core;
using Ryba.Data;

var host = Host.CreateDefaultBuilder(args)
    .AddDiscordService(services =>
    {
        var configuration = services.GetRequiredService<IConfiguration>();

        return configuration.GetValue<string?>("Ryba:BotToken") ??
               throw new InvalidOperationException(
                   "No bot token has been provided. Set the Ryba__BotToken environment variable to a valid token.");
    })
    .ConfigureServices((context, services) => services
                .AddDbContext<RybaContext>(
                    options => options.UseNpgsql(
                        context.Configuration.GetValue<string>("Ryba:ConnectionString"),
                        options => options.MigrationsAssembly("Ryba.Data")))
                .AddDiscordCommands(enableSlash: true)
                .AddCommandTree()
                .WithCommandGroup<Ryba.Bot.Commands.EvalCommands>()
                .Finish()
                .AddPostExecutionEvent<Ryba.Bot.PostEvent>()
                .AddSingleton(new FluentLocalizationService())
                .AddInteractivity()
                .AddPagination())
    .ConfigureLogging(c => c.AddConsole()
        .AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning)
        .AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning))
    .UseConsoleLifetime()
    .Build();

var services = host.Services;
var log = services.GetRequiredService<ILogger<Program>>();
var configuration = services.GetRequiredService<IConfiguration>();

Snowflake? debugServer = null;

#if DEBUG
var debugServerString = configuration.GetValue<string?>("Ryba:DebugServer");
if (debugServerString is not null)
{
    if (!DiscordSnowflake.TryParse(debugServerString, out debugServer))
    {
        log.LogWarning("Failed to parse debug server from environment");
    }
}
#endif

var slashService = services.GetRequiredService<SlashService>();

var checkSlashSupport = slashService.SupportsSlashCommands();
if (!checkSlashSupport.IsSuccess)
{
    log.LogWarning("The registered commands of the bot don't support slash commands: {Reason}",
        checkSlashSupport.Error?.Message);
}
else
{
    var updateSlash = await slashService.UpdateSlashCommandsAsync(debugServer);
    if (!updateSlash.IsSuccess)
    {
        log.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error?.Message);
    }
}

await host.RunAsync();
