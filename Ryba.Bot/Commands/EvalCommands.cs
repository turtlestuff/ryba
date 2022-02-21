using System.Data;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;
using Ryba.Bot.Extensions;
using Ryba.Data;

namespace Ryba.Bot.Commands;

public sealed class EvalCommands : CommandGroup
{
    private readonly FeedbackService feedbackService;
    private readonly ICommandContext context;
    private readonly IDiscordRestChannelAPI restChannelApi;
    private readonly InteractionContext interactionContext;
    private readonly RybaContext db;

    public EvalCommands(FeedbackService feedbackService, ICommandContext context, IDiscordRestChannelAPI restChannelApi, InteractionContext interactionContext, RybaContext db)
    {
        this.feedbackService = feedbackService;
        this.context = context;
        this.restChannelApi = restChannelApi;
        this.interactionContext = interactionContext;
        this.db = db;
    }

    [Command("eval")]
    public async Task<IResult> PostEvalAsync(string expression)
    {
        var result = new DataTable().Compute(expression, "");
        var embed = new Embed(
            Title: "Result",
            Description: result?.ToString() ?? "<null>",
            Footer: new EmbedFooter(expression));

        var reply = await feedbackService.SendContextualEmbedAsync(embed);
        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }

    [Command("pinlast")]
    public async Task<IResult> PinLastAsync()
    {
        var messages = await restChannelApi.GetChannelMessagesAsync(context.ChannelID, limit: 2);
        var message = messages.Entity.Where(m => !m.Interaction.HasValue || m.Interaction.Value.ID != interactionContext.ID).First();
        var user = db.GetOrCreateUser(context.User.ID);
        user.PortablePins.Add(new PortablePin
        {
            Channel = context.ChannelID.ToString(),
            Message = message.ID.ToString()
        });

        await db.SaveChangesAsync();
        await feedbackService.SendContextualNeutralAsync("PINNED " + message.Content);
        return Result.FromSuccess();
    }

    [Command("pins")]
    public async Task<IResult> PinsAsync()
    {
        var embedFields = new List<IEmbedField>();

        var user = await db.Users.Include(u => u.PortablePins).FirstOrDefaultAsync(u => u.Id == context.User.ID.ToString());
        if (user == null)
        {
            await feedbackService.SendContextualNeutralAsync("No pins");
            return Result.FromSuccess();
        }

        foreach (var pin in user.PortablePins)
        {
            var message = await restChannelApi.GetChannelMessageAsync(
                channelID: new Snowflake(ulong.Parse(pin.Channel)),
                messageID: new Snowflake(ulong.Parse(pin.Message)));
            embedFields.Add(new EmbedField(
                Name: message.Entity.Author.Username,
                Value: message.Entity.Content));
        }

        await feedbackService.SendContextualEmbedAsync(new Embed(Fields: embedFields));
        return Result.FromSuccess();
    }

}
