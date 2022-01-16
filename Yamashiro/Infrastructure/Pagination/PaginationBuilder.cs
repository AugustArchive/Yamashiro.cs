using Yamashiro.Infrastructure.Conditions;
using System.Threading.Tasks;
using Yamashiro.Services;
using Discord.WebSocket;
using Discord.Rest;
using System.Linq;
using Discord;

namespace Yamashiro.Infrastructure.Pagination
{
    /// <summary>
    /// Builder for managing pagination
    /// </summary>
    public class PaginationBuilder
    {
        public PaginationBuilderOptions Options;
        public YamashiroCommandContext Context { get; set; }
        public PaginationService Service { get; set; }
        public BaseCondition<SocketReaction> Condition { get; }
        public IUserMessage Message { get; private set; }

        private int PageSize { get; set; }
        private PaginatedMessage pager;
        private int PageId;
        
        public PaginationBuilder(
            YamashiroCommandContext ctx, 
            PaginatedMessage msg, 
            PaginationService service, 
            PaginationBuilderOptions options,
            BaseCondition<SocketReaction> condition = null)
        {
            Condition = condition;
            Service = service;
            Context = ctx;
            Options = options ?? PaginationBuilderOptions.Default;
            PageId = 1;
            pager = msg;

            SetPageSize();
        }

        private void SetPageSize() => PageSize = pager.Pages.Count();

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Service.AddBuilder(this, message);

            _ = Task.Run(async() => 
            {
                if (Options.ReactionList.First) await message.AddReactionAsync(Options.First);
                if (Options.ReactionList.Backward) await message.AddReactionAsync(Options.Backward);
                if (Options.ReactionList.Forward) await message.AddReactionAsync(Options.Forward);
                if (Options.ReactionList.Last) await message.AddReactionAsync(Options.Last);
                if (Options.ReactionList.Trash) await message.AddReactionAsync(Options.Trash);
            });

            AddTimeout(message);
        }

        public void AddTimeout(RestUserMessage message)
        {
            _ = Task.Delay(120000).ContinueWith(_ => 
            {
                Service.RemoveBuilder(message);
                Message.DeleteAsync();
            });
        }

        public async Task<bool> HandlePaginationAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;
            if (emote.Equals(Options.First)) PageId = 1;
            else if (emote.Equals(Options.Forward))
            {
                if (PageId >= PageSize) return false;
                PageId++;
            }
            else if (emote.Equals(Options.Backward))
            {
                if (PageId <= 1) return false;
                --PageId;
            }
            else if (emote.Equals(Options.Last)) PageId = PageSize;
            else if (emote.Equals(Options.Trash))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }

            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderOutAsync().ConfigureAwait(false);
            return false;
        }

        protected Embed BuildEmbed()
        {
            var current = pager.Pages.ElementAt(PageId - 1);
            return current.Embed;
        }

        private Task RenderOutAsync()
        {
            var embed = BuildEmbed();
            return Message.ModifyAsync(m =>
            {
                m.Embed = embed;
            });
        }
    }
}