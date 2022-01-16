using System.Threading.Tasks;
using Discord.WebSocket;
using Yamashiro.Logging;
using Discord;
using System;

namespace Yamashiro.Services
{
    public class EventHandlingService
    {
		private readonly DiscordSocketClient client;
		private readonly DatabaseService database;
		private readonly Logger logger;

		public EventHandlingService(DiscordSocketClient _client, DatabaseService service)
		{
			database = service;
			logger = new Logger("EventHandler");
			client = _client;

			client.Disconnected += OnDisconnectedAsync;
			client.UserJoined += OnUserJoinAsync;
			client.Connected += OnConnectedAsync;
			client.UserLeft += OnUserLeftAsync;
			client.Ready += OnReadyAsync;
		}

		private async Task OnDisconnectedAsync(Exception ex) => logger.Error(ex);
		private async Task OnUserJoinAsync(SocketGuildUser user)
		{
			var embed = new EmbedBuilder()
				.WithAuthor(x =>
				{
					x.WithIconUrl(user.GetAvatarUrl(ImageFormat.Png, 1024));
					x.WithName($"| User {user.Username}#{user.Discriminator} has joined");
				})
				.WithColor(new Color(255, 64, 160))
				.WithFooter(x =>
				{
					x.WithText($"Now at {user.Guild.MemberCount} members!");
				});

			await database.CreateUser(user.Id);

			var channel = user.Guild.GetTextChannel(529593466729267200);
			await channel.SendMessageAsync(embed: embed.Build());
		}
		private async Task OnUserLeftAsync(SocketGuildUser user)
		{
			var embed = new EmbedBuilder()
				.WithAuthor(x =>
				{
					x.WithIconUrl(user.GetAvatarUrl(ImageFormat.Png, 1024));
					x.WithName($"| User {user.Username}#{user.Discriminator} has left");
				})
				.WithColor(new Color(255, 64, 160))
				.WithFooter(x =>
				{
					x.WithText($"Now at {user.Guild.MemberCount} members...");
				});

			await database.DeleteUser(user.Id);
			var channel = user.Guild.GetTextChannel(529593466729267200);
			await channel.SendMessageAsync(embed: embed.Build());
		}
		private async Task OnConnectedAsync() => logger.Info("Established a connection to Discord via WebSocket!");
		private async Task OnReadyAsync()
		{
			logger.Info($"Connected to Discord as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
			var guild = client.GetGuild(382725233695522816);

			await client.SetGameAsync($"over {guild.MemberCount} cuties | shiro help", type: ActivityType.Watching);
		}
    }
}