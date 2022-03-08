using System.Data;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity.Services;
using Remora.Discord.Pagination.Extensions;
using Remora.Rest.Core;
using Remora.Results;
using Ryba.Bot.Extensions;
using Ryba.Data;

namespace Ryba.Bot.Commands;

public sealed class EvalCommands : CommandGroup
{
    readonly FeedbackService feedbackService;
    readonly InteractiveMessageService interactiveMessage;
    readonly ICommandContext context;
    readonly IDiscordRestChannelAPI restChannelApi;
    readonly InteractionContext interactionContext;
    readonly RybaContext db;
    readonly FluentLocalizationService tr;

    public EvalCommands(FeedbackService feedbackService, InteractiveMessageService interactiveMessage,
        ICommandContext context, IDiscordRestChannelAPI restChannelApi, 
        InteractionContext interactionContext, RybaContext db, FluentLocalizationService tr)
    {
        this.feedbackService = feedbackService;
        this.interactiveMessage = interactiveMessage;
        this.context = context;
        this.restChannelApi = restChannelApi;
        this.interactionContext = interactionContext;
        this.db = db;
        this.tr = tr;
    }

    [Command("eval")]
    public async Task<IResult> PostEvalAsync(string expression)
    {
        var lang = db.GetOrCreateUser(context.User.ID).Language;
        var result = new DataTable().Compute(expression, "");
        var embed = new Embed(
            Title: tr[lang, "bot-eval-result"],
            Description: result?.ToString() ?? "<null>",
            Footer: new EmbedFooter(expression));

        var reply = await feedbackService.SendContextualEmbedAsync(embed);
        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }

    [Command("pinlast")]
    public async Task<IResult> PinLastAsync()
    {
        var lang = db.GetOrCreateUser(context.User.ID).Language;
        var messages = await restChannelApi.GetChannelMessagesAsync(context.ChannelID, limit: 2);
        var message = messages.Entity.Where(m => !m.Interaction.HasValue || m.Interaction.Value.ID != interactionContext.ID).First();
        var user = db.GetOrCreateUser(context.User.ID);
        user.PortablePins.Add(new PortablePin
        {
            Channel = context.ChannelID.ToString(),
            Message = message.ID.ToString()
        });

        await db.SaveChangesAsync();
        await feedbackService.SendContextualNeutralAsync(tr[lang, "bot-pins-pinned"] + message.Content);
        return Result.FromSuccess();
    }

    [Command("pins")]
    public async Task<IResult> PinsAsync()
    {
        var lang = db.GetOrCreateUser(context.User.ID).Language;
        var embedFields = new List<IEmbedField>();

        var user = await db.Users.Include(u => u.PortablePins).FirstOrDefaultAsync(u => u.Id == context.User.ID.ToString());
        if (user == null)
        {
            await feedbackService.SendContextualNeutralAsync(tr[lang, "bot-pins-no-pins"]);
            return Result.FromSuccess();
        }

        var groups = user.PortablePins.Select((p, i) => (p, i)).GroupBy(x => x.i / 10);
        var pages = new List<Embed>();
        foreach (var group in groups)
        {
            var fields = new List<EmbedField>();
            foreach (var (pin, _) in group)
            {
                var msg = await restChannelApi.GetChannelMessageAsync(new(ulong.Parse(pin.Channel)), new(ulong.Parse(pin.Message)), CancellationToken);
                fields.Add(new(msg.Entity.Author.Username, msg.Entity.Content, true));
            }
            var embed = new Embed
            {
                Title = tr[lang, "pins"],
                Description = tr[lang, "bot-pins-num-pins", ("count", user.PortablePins.Count)],
                Fields = fields
            };
            pages.Add(embed);
        }

        await interactiveMessage.SendContextualPaginatedMessageAsync(context.User.ID, pages, ct: CancellationToken);
        return Result.FromSuccess();
    }

}
