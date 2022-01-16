using Yamashiro.Infrastructure.Pagination.Conditions;
using Yamashiro.Infrastructure.Pagination;
using Yamashiro.Infrastructure.Conditions;
using System.Collections.Generic;
using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Yamashiro.Services
{
    public class PaginationService
    {
        private readonly Dictionary<ulong, PaginationBuilder> builders;
        private readonly DiscordSocketClient client;

        public PaginationService(DiscordSocketClient _client)
        {
            builders = new Dictionary<ulong, PaginationBuilder>();
            client = _client;

            client.ReactionAdded += OnReactionAddAsync;
        }

        public void AddBuilder(PaginationBuilder builder, IUserMessage msg) => builders[msg.Id] = builder;
        public void RemoveBuilder(IUserMessage msg) => builders.Remove(msg.Id);
        public void RemoveBuilder(ulong id) => builders.Remove(id);
        public PaginationBuilder CreateBuilder(YamashiroCommandContext ctx, PaginatedMessage msg) => new PaginationBuilder(ctx, msg, this, PaginationBuilderOptions.Default, new ReactionSourceCondition());

        private async Task OnReactionAddAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == client.CurrentUser.Id) return;
            if (!builders.TryGetValue(message.Id, out var builder)) return;

            var result = await builder.Condition.PreconditionAsync(builder.Context, reaction).ConfigureAwait(false);
            if (!result) return;

            _ = Task.Run(async() =>
            {
                var result = await builder.HandlePaginationAsync(reaction).ConfigureAwait(false);
                if (result) RemoveBuilder(message.Id);
            });
        }
    }
}