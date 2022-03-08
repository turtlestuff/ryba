using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;

namespace Ryba.Bot;

class PostEvent : IPostExecutionEvent
{
    readonly ILogger<PostEvent> logger;

    public PostEvent(ILogger<PostEvent> logger)
    {
        this.logger = logger;
    }

    public Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct)
    {
        if (!commandResult.IsSuccess)
        {
            logger.LogError(new Exception(commandResult.Error?.Message), "An unexpected error occurred when executing a command.");
            
        }

        // TODO: "This interaction failed"

        return Task.FromResult(Result.FromSuccess());
    }
}