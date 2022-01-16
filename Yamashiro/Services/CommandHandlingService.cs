using Microsoft.Extensions.Configuration;
using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Yamashiro.Logging;
using Discord.WebSocket;
using Discord.Commands;
using System;

namespace Yamashiro.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient client;
        private readonly IConfigurationRoot config;
        private readonly IServiceProvider provider;
        private readonly CommandService commands;
        private readonly DatabaseService db;
        private readonly Logger logger;

        public CommandHandlingService(
            DiscordSocketClient _client,
            CommandService _commands,
            IServiceProvider _provider,
            IConfigurationRoot _config,
            DatabaseService _database)
        {
            commands = _commands;
            provider = _provider;
            client = _client;
            config = _config;
            logger = new Logger("CommandHandler");
            db = _database;

            client.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage msg)
        {
            // Don't do anything if it's a WS user message
            if (!(msg is SocketUserMessage)) return;

            var m = (msg as SocketUserMessage);

            // Check if the message is null
            if (m == null) return;

            // Check for bots and the current user
            if (m.Author.Id == client.CurrentUser.Id || m.Author.IsBot) return;

            // Don't execute in DMs
            if (!(m.Channel is SocketGuildChannel)) return;

            // Create a new command context
            var ctx = new YamashiroCommandContext(db, client, m);

            var user = await db.GetUser(m.Author.Id);
            if (user == null)
            {
                await db.CreateUser(m.Author.Id);
                user = await db.GetUser(m.Author.Id);
            }

            // Get the argument position
            var pos = 0;

            // Check if the message has a valid prefux
            if (m.HasStringPrefix(config["discord:prefix"], ref pos) || m.HasMentionPrefix(client.CurrentUser, ref pos))
            {
                var result = await commands.ExecuteAsync(ctx, pos, provider);
                var reason = "";

                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnmetPrecondition:
                            {
                                reason = $"Precondition was not met ({result.ErrorReason})";
                            }
                            break;

                        case CommandError.Unsuccessful:
                            {
                                reason = $"Command broke during runtime, ping <@280158289667555328> for assistance!\n{result.ErrorReason}";
                            }
                            break;

                        case CommandError.ParseFailed:
                            {
                                reason = $"Unable to parse arguments ({result.ErrorReason})";
                            }
                            break;

                        case CommandError.BadArgCount:
                            {
                                reason = "Command gave too little or many arguments!";
                            }
                            break;

                        case CommandError.ObjectNotFound:
                            {
                                reason = "Command object was not found, contact <@280158289667555328> now!";
                            }
                            break;
                        case CommandError.UnknownCommand: break;
                        case CommandError.MultipleMatches:
                            {
                                reason = $"Found multiple commands ({result.ErrorReason})";
                            } break;
                    }

                    logger.Error(reason);
                    if (reason != "") await ctx.Channel.SendMessageAsync(reason);
                }
            }
        }
    }
}