using Yamashiro.Infrastructure.Timers;
using Yamashiro.Database.Redis;
using System.Threading.Tasks;
using ServiceStack.Redis;
using Yamashiro.Logging;
using Discord.WebSocket;
using System.Text;
using Humanizer;
using Discord;
using System;

namespace Yamashiro.Services
{
    public class GiveawayService
    {
        private readonly DiscordSocketClient client;
        private readonly IRedisClient redis;
        private readonly Logger logger;

        public GiveawayService(DiscordSocketClient sock, RedisManagerPool pool)
        {
            client = sock;
            logger = new Logger("GiveawayService");
            redis = pool.GetClient();
        }

        private async Task UpdateMessage(ulong messageID)
        {
            var giveaway = redis.Get<GiveawayModel>($"giveaway:{messageID}");
            var message = await client
                .GetGuild(382725233695522816)
                .GetTextChannel(525347563809931314)
                .GetMessageAsync(messageID) as SocketUserMessage;

            if (message == null) return;

            var description = new StringBuilder(":tada: **Giveaway** :tada:\n")
                .AppendLine()
                .AppendLine($"Prize: {giveaway.Prize}")
                .AppendLine($"Winner Count: {giveaway.WinnerCount}")
                .AppendLine($"Entries: {giveaway.Entries}")
                .AppendLine()
                .AppendLine($"Ends at: {DateTime.Now.Millisecond - giveaway.EndAt.Milliseconds().TotalMilliseconds}");

            await message.ModifyAsync(item => 
            {
                item.Content = description.ToString();
            });
        }

        public async Task<(bool, string)> StartGiveaway(SocketUser user, string prize, int winnerCount, DateTime endTime)
        {
            if (winnerCount == 0) return (false, "Winner count is too low");
            if (winnerCount > 5) return (false, $"Winner count is too high ({winnerCount - 5} over the limit)");

            var channel = client
                .GetGuild(382725233695522816)
                .GetTextChannel(525347563809931314);

            logger.Info($"Now starting giveaway with prize {prize} ({winnerCount} winners)");
            var description = new StringBuilder(":tada: **Giveaway** :tada:\n")
                .AppendLine()
                .AppendLine($"Prize: {prize}")
                .AppendLine($"Winner Count: {winnerCount}")
                .AppendLine($"Entries: None")
                .AppendLine()
                .AppendLine($"Ends at: {(DateTime.Now.Millisecond + endTime.Millisecond)}");

            var message = await channel.SendMessageAsync(text: description.ToString());
            var batch = redis.Set($"giveaway:{message.Id}", new GiveawayModel 
            { 
                CurrentStatus = Status.Ongoing,
                WinnerCount = winnerCount,
                CreatedAt = DateTime.Now.Millisecond,
                MessageID = message.Id,
                CreatorID = user.Id,
                Entries = 0,
                Prize = prize,
                EndAt = endTime.Millisecond
            });

            // We don't care about the key since we'll iterate over it
            // when the bot starts
            redis.Set($"timer:giveaways:{message.Id}", message.Id);

            client.ReactionAdded += async (message, channel, reaction) =>
            {
                if (channel.Id == 525347563809931314)
                {
                    var m = await message.DownloadAsync();
                    var giveaway = redis.Get<GiveawayModel>($"giveaway:{m.Id}");

                    // Don't do anything if the giveaway returns null
                    if (giveaway == null) return;

                    if (reaction.UserId != client.CurrentUser.Id)
                    {
                        var entries = giveaway.Entries++;
                    }
                }
            };

            client.ReactionRemoved += async (message, channel, reaction) =>
            {
                if (channel.Id == 525347563809931314)
                {
                    var m = await message.DownloadAsync();
                    var giveaway = redis.Get<GiveawayModel>($"giveaway:{m.Id}");

                    // Don't do anything if the giveaway returns null
                    if (giveaway == null) return;

                    if (reaction.UserId != client.CurrentUser.Id)
                    {
                        var entries = giveaway.Entries--;
                    }
                }
            };

            var timer = SetInterval.Create(async () => 
            {
                var giveaway = redis.Get<GiveawayModel>($"giveaway:{message.Id}");
                if (giveaway.CurrentStatus == Status.Cancelled)
                {
                    await message.ModifyAsync(item => 
                    {
                        item.Content = new StringBuilder(":x: **Giveaway Cancelled** :x:\n")
                            .AppendLine("Giveaway has been cancelled by the creator...")
                            .ToString();
                    });

                    redis.Delete($"giveaway:{message.Id}");
                }
                else
                {

                }
            }, TimeSpan.FromSeconds(5));

            return (true, null);
        }
    }
}