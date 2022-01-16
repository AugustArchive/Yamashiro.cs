using System.Collections.Generic;
using TwitchLib.PubSub.Events;
using System.Threading.Tasks;
using Yamashiro.Logging;
using TwitchLib.PubSub;
using Discord.Webhook;
using Discord;

namespace Yamashiro.Services
{
    public class PubSubService
    {
        private readonly DiscordWebhookClient webhook;
        private readonly TwitchPubSub client;
        private readonly Logger logger;

        public PubSubService()
        {
            webhook = new DiscordWebhookClient(709502386615484495, "XhKRZnd4wWsl6CjUtNdGXEECRNVsDwPdm7_TGGMos5Lo1KZXBusfX7YjW6NaLZQccqJM");
            logger = new Logger("TwitchPubSub");
            client = new TwitchPubSub();

            client.OnStreamDown += Service_StreamDown;
            client.OnStreamUp += Service_StreamUp;
            client.ListenToVideoPlayback("iuyn_");
        }

        private void Service_StreamDown(object sender, OnStreamDownArgs args)
        {
            logger.Warn($"Stream went down! (Uptime: {args.ServerTime})");
            var embed = new EmbedBuilder()
                .WithTitle("[ Stream Status ]")
                .WithDescription($"Stream went down with uptime **{args.ServerTime},** thanks for watching!")
                .WithColor(new Color(255, 64, 160));

            var embeds = new List<Embed>
                  {
                      embed.Build()
                  };

            webhook.SendMessageAsync(embeds: embeds).ConfigureAwait(false);
        }

        public async Task Connect()
        {
            logger.Info("Now listening to Publish/Subscribe events from Twitch!");
            client.Connect();

            var embed = new EmbedBuilder()
                .WithTitle("[ Stream Status ]")
                .WithDescription("Pub/Sub service is online")
                .WithColor(new Color(255, 64, 160));

            var embeds = new List<Embed>
                  {
                      embed.Build()
                  };

            await webhook.SendMessageAsync(embeds: embeds);
        }

        public async Task Disconnect()
        {
            logger.Warn("Pub/Sub client is now going offline");
            client.Disconnect();

            var embed = new EmbedBuilder()
                .WithTitle("[ Stream Status ]")
                .WithDescription("Pub/Sub service is offline")
                .WithColor(new Color(255, 64, 160));

            var embeds = new List<Embed>
                {
                      embed.Build()
                };

            await webhook.SendMessageAsync(embeds: embeds);
        }

        private void Service_StreamUp(object sender, OnStreamUpArgs args)
        {
            logger.Info($"We have started streaming with delay {args.PlayDelay}");
            var embed = new EmbedBuilder()
                .WithTitle("[ Stream Status ]")
                .WithDescription($"Stream is now online with **{args.PlayDelay}** delay!")
                .WithColor(new Color(255, 64, 160));

            var embeds = new List<Embed>
                  {
                      embed.Build()
                  };

            webhook.SendMessageAsync(embeds: embeds).ConfigureAwait(false);
        }
    }
}
