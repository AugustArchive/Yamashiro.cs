namespace Yamashiro.Database.Redis
{
    public class GiveawayModel
    {
        public Status CurrentStatus { get; set; }
        public int WinnerCount { get; set; }
        public long CreatedAt { get; set; }
        public ulong MessageID { get; set; }
        public ulong CreatorID { get; set; }
        public string Prize { get; set; }
        public int Entries { get; set; }
        public long EndAt { get; set; }
    }

    public enum Status
    {
        Cancelled = -1,
        Ongoing = 0
    }
}