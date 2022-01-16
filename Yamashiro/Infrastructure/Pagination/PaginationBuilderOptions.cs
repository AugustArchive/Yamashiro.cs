using Discord;
using System;

namespace Yamashiro.Infrastructure.Pagination
{
    public class PaginationBuilderOptions
    {
        public IReactionList ReactionList { get; set; } = new IReactionList
        {
            Forward = true,
            Backward = true,
            First = true,
            Last = true,
            Trash = true
        };
        public string FooterFormat { get; set; } = "Page [{0}/{1}";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);
        public IEmote Backward = new Emoji("◀");
        public IEmote Forward = new Emoji("▶");
        public IEmote First = new Emoji("⏮");
        public IEmote Trash = new Emoji("🗑️");
        public IEmote Last = new Emoji("⏭");

        public static PaginationBuilderOptions Default { get; set; } = new PaginationBuilderOptions();
    }
}
