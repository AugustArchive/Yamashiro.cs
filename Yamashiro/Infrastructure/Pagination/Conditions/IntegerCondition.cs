using Yamashiro.Infrastructure.Conditions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Yamashiro.Infrastructure.Pagination.Conditions
{
    public class IntegerCondition: BaseCondition<SocketMessage>
    {
        public Task<bool> PreconditionAsync(YamashiroCommandContext ctx, SocketMessage arg)
        {
            var result = int.TryParse(arg.Content, out _);
            return Task.FromResult(result);
        }
    }
}