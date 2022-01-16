using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using System;

namespace Yamashiro.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;

        public LoggingService(DiscordSocketClient discord, CommandService _service)
        {
            commands = _service;
            client = discord;

            commands.Log += OnDiscordLogAsync;
            client.Log += OnDiscordLogAsync;
        }

        private Task OnDiscordLogAsync(LogMessage msg)
        {
            var text = $"[{DateTime.Now.ToString("hh:mm:ss")}] [{msg.Severity}] [{msg.Source}] <=> {msg.Exception?.ToString() ?? msg.Message}";
            return Console.Out.WriteLineAsync(text);
        }
    }
}