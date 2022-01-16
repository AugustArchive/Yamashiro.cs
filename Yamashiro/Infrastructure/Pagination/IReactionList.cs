namespace Yamashiro.Infrastructure.Pagination
{
    public class IReactionList
    {
        public bool Backward { get; set; }
        public bool Forward { get; set; }
        public bool Trash { get; set; }
        public bool First { get; set; }
        public bool Last { get; set; }
    }
}