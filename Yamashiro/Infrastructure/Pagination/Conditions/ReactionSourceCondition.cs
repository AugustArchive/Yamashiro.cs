using Yamashiro.Infrastructure.Conditions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Yamashiro.Infrastructure.Pagination.Conditions
{
    class ReactionSourceCondition: BaseCondition<SocketReaction>
    {
        public Task<bool> PreconditionAsync(YamashiroCommandContext ctx, SocketReaction reaction) => Task.FromResult(reaction.UserId == ctx.User.Id);
    }
}