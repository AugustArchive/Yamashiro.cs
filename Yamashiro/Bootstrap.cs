using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Yamashiro.Utilities;
using System.Diagnostics;
using Yamashiro.Services;
using ServiceStack.Redis;
using Yamashiro.Logging;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using System;

namespace Yamashiro
{
    public class Bootstrap
    {
        private readonly Process process = Process.GetCurrentProcess();
        private IServiceProvider _provider;
        public IConfigurationRoot config;

        public Bootstrap()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json");

            config = builder.Build();
        }
        
        public async Task StartAsync()
        {
            ConfigureEventsForProcess();
            var services = new ServiceCollection();
            ConfigureAllServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<DatabaseService>();
            provider.GetRequiredService<EventHandlingService>();
            provider.GetRequiredService<CommandHandlingService>();

            var pubsub = provider.GetRequiredService<PubSubService>();
            await pubsub.Connect();

            _provider = provider;
            await provider.GetRequiredService<BootstrapService>().BootstrapAsync();
            await Task.Delay(-1);
        }

        private void ConfigureAllServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Verbose
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            }))
            .AddSingleton(new RedisManagerPool("localhost:6379?db=7"))
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<EventHandlingService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<DatabaseService>()
            .AddSingleton<BootstrapService>()
            .AddSingleton<EvaluationService>()
            .AddSingleton<PaginationService>()
            .AddSingleton<HttpRequest>()
            .AddSingleton<PubSubService>()
            .AddSingleton<GiveawayService>()
            .AddSingleton(config);
        }

        private void ConfigureEventsForProcess()
        {
            process.Exited += Process_Exited;
        }

        private async void Process_Exited(object sender, EventArgs args)
        {
            var logger = new Logger($"Process #{process.Id}");
            logger.Warn($"Process has exited with code {process.ExitCode} with uptime {Math.Round((process.ExitTime - process.StartTime).TotalMilliseconds)}");

            var pubsub = _provider.GetRequiredService<PubSubService>();
            var client = _provider.GetRequiredService<DiscordSocketClient>();

            await pubsub.Disconnect();
            client.Dispose();
        }
    }
}
