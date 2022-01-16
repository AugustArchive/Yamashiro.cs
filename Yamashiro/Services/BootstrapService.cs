using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Yamashiro.Logging;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Discord;
using System;

namespace Yamashiro.Services
{
    public class BootstrapService
    {
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider provider;
        private readonly IConfigurationRoot config;
        private readonly CommandService commands;
        private readonly Logger logger;

        public BootstrapService(
            IServiceProvider _provider,
            DiscordSocketClient _discord,
            CommandService _service,
            IConfigurationRoot _config)
        {
            provider = _provider;
            commands = _service;
            logger = new Logger("Bootstrap");
            client = _discord;
            config = _config;
        }

        public async Task BootstrapAsync()
        {
            var token = config["discord:token"];
            if (string.IsNullOrWhiteSpace(token)) throw new Exception("Enter the bot token u dummy...");

            logger.Info("Bootstrapping is occuring...");
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
        }
    }
}