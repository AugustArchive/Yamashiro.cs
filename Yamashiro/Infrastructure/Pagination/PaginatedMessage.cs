using System.Collections.Generic;
using Discord;

namespace Yamashiro.Infrastructure.Pagination
{
    /// <summary>
    /// Represents a "paginated" message
    /// </summary>
    public class PaginatedMessage
    {
        public List<PaginatedPage> Pages { get; set; }
        public Embed CurrentEmbed { get; set; }
        public string Content { get; set; }

        public PaginatedMessage(List<PaginatedPage> pages = null)
        {
            Pages = pages ?? new List<PaginatedPage>();
        }

        public PaginatedMessage AddPage(PaginatedPage page)
        {
            Pages.Add(page);
            return this;
        }
    }

    public class PaginatedPage
    {
        public Embed Embed { get; set; }
        public string Content { get; set; }
    }
}