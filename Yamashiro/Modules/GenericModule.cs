using System.Runtime.Versioning;
using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Humanizer.Localisation;
using Yamashiro.Services;
using System.Diagnostics;
using Yamashiro.Database;
using System.Reflection;
using Discord.Commands;
using MongoDB.Bson;
using System.Linq;
using System.Text;
using Humanizer;
using Discord;
using System;

namespace Yamashiro.Modules
{
    [Name("Generic")]
    [Summary("Generic commands, nothing really special...")]
    public class GenericModule: YamashiroModule
    {
        private readonly CommandService commands;
        private readonly DatabaseService database;
        public GenericModule(DatabaseService db, CommandService service)
        {
            database = db;
            commands = service;
        }

        [Command("about")]
        [Summary("Show a bit of information about Yamashiro")]
        async Task About()
        {
            var db = database.GetDatabase();
            var coll = db.GetCollection<UserModel>("users");
            var registered = await coll.CountDocumentsAsync(new BsonDocument());
            var home = Context.Client.GetGuild(382725233695522816);
            var process = Process.GetCurrentProcess();
            var heap = Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
            var framework = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? ".NET Core";

            if (framework != ".NET Core")
            {
                var splitted = framework.Split(',');
                var name = splitted[0].Replace(".NETCoreApp", ".NET Core");
                var version = splitted[1].Split(',');
                version = version[0].Split('=');

                framework = $"{name} (**{version[1]}**)";
            }

            var message = new StringBuilder()
                .AppendLine("> Hello Admiral-kun, I am Yamashiro. I am a helper of this guild because automated stuff is good stuff. Below is just useless information if you're into that.")
                .AppendLine("")
                .AppendLine($"**Users Registered**: {registered}")
                .AppendLine($"**Member Count**: {home.MemberCount}")
                .AppendLine($"**Modules Registered**: {commands.Modules.Count()}")
                .AppendLine($"**Commands Registered**: {commands.Commands.Count()}")
                .AppendLine($"**Threads**: {Process.GetProcesses().Sum(x => x.Threads.Count)}")
                .AppendLine($"**Memory Usage**: {heap}MiB")
                .AppendLine($"**Discord.Net Version**: {DiscordConfig.Version}")
                .AppendLine($"**Framework**: {framework}")
                .AppendLine($"**Database Calls**: {database.Calls}")
                .ToString();

            await ReplyAsync(message);
        }

        [Command("help")]
        [Summary("Grabs a list of all commands of Yamashiro")]
        async Task Help(string cmdOrMod = null)
        {
            if (cmdOrMod is null)
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.WithIconUrl(Context.User.GetAvatarUrl(ImageFormat.Png, 1024));
                        x.WithName("| Commands List");
                    })
                    .WithDescription("Use `shiro help <cmdOrMod>` to get help on a command or list a module's commands!")
                    .WithColor(new Color(255, 64, 160));

                foreach (var module in this.commands.Modules)
                {
                    // Don't know why Discord.Net adds the main module base to it but whatever
                    if (module.Name == "YamashiroModule") continue;

                    var allCommands = $"`{string.Join("`, `", module.Commands.Select(x => x.Aliases.First()))}`";
                    embed.AddField(module.Name, allCommands, false);
                }

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var targetMod = commands.Modules.FirstOrDefault(x => x.Name.Equals(cmdOrMod));
                var targetCmd = commands.Commands.FirstOrDefault(x => x.Aliases.Contains(cmdOrMod));

                if (targetMod is null && targetCmd is null) await ReplyAsync($"Unable to find commmand or module: **{cmdOrMod}**");
                if (targetMod is null && targetCmd != null)
                {
                    var embed = new EmbedBuilder()
                        .WithAuthor(x =>
                        {
                            x.WithIconUrl(Context.User.GetAvatarUrl(ImageFormat.Png, 1024));
                            x.WithName($"| Command {targetCmd.Name}");
                        })
                        .WithColor(new Color(255, 64, 160));

                    var sb = new StringBuilder("```apache\n")
                        .AppendLine($"Module: {targetCmd.Module.Name}");

                    var aliases = targetCmd.Aliases.Where(x => !(targetCmd.Name == x));

                    if (!string.IsNullOrEmpty(targetCmd.Summary)) sb.AppendLine($"Description: {targetCmd.Summary}");
                    if (aliases.Count() > 0) sb.AppendLine($"Aliases: {string.Join(", ", aliases)}");
                    if (targetCmd.Parameters.Count > 0)
                    {
                        var usage = GetCommandParameters(targetCmd);
                        sb.AppendLine($"Syntax: {usage}");
                    }
                    else sb.AppendLine($"Syntax: shiro {targetCmd.Name}");

                    sb.AppendLine("```");
                    embed.WithDescription(sb.ToString());
                    await ReplyAsync(embed: embed.Build());
                }
                if (targetMod != null && targetCmd is null)
                {
                    var embed = new EmbedBuilder()
                        .WithAuthor(x =>
                        {
                            x.WithIconUrl(Context.User.GetAvatarUrl(ImageFormat.Png, 1024));
                            x.WithName($"| Module {targetMod.Name}");
                        })
                        .WithColor(new Color(255, 64, 160));

                    var sb = new StringBuilder();

                    if (!string.IsNullOrEmpty(targetMod.Summary)) sb.AppendLine($"**{targetMod.Summary}**");
                    else sb.AppendLine($"**Module doesn't have a description.**");

                    sb
                        .AppendLine("")
                        .AppendLine($"`{string.Join("`, `", targetMod.Commands.Select(x => x.Aliases.First()))}`");

                    embed.WithDescription(sb.ToString());
                    if (targetMod.Parent != null) embed.AddField("Parent Module", targetMod.Parent.Name, true);
                    if (targetMod.Submodules.Count > 0)
                    {
                        var submodules = $"`{string.Join("`, `", targetMod.Submodules.Select(x => x.Name))}`";
                        embed.AddField("Submodules", submodules, true);
                    }

                    await ReplyAsync(embed: embed.Build());
                }
            }
        }

        [Command("ping")]
        [Summary("Shows the latency from Discord to the bot.")]
        async Task Ping()
        {
            var sw = new Stopwatch();
            sw.Start();
            var msg = await Context.Channel.SendMessageAsync(":ping_pong: **| Pinging...**");
            sw.Stop();
            await msg.ModifyAsync(x =>
            {
                x.Content = $":ping_pong: **| Pong! `{sw.ElapsedMilliseconds.ToString()}ms`**";
            });
        }

        [Command("uptime")]
        [Summary("Shows the current uptime of Yamashiro")]
        Task Uptime() => ReplyAsync($":gear: **| {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Month)}**");

        private string GetCommandParameters(CommandInfo cmd)
        {
            var sb = new StringBuilder($"shiro {cmd.Name} ");

            // it's hacky but it'll do i guess lol
            var iterations = 0;
            foreach (var param in cmd.Parameters)
            {
                var oPad = iterations == 0 ? "" : " ";

                if (param.IsOptional)
                {
                    sb.Append($"{oPad}[{param.Name}]");
                    iterations++;
                }

                if (!param.IsOptional)
                {
                    sb.Append($"{oPad}<{param.Name}>");
                    iterations++;
                }

                if (param.IsRemainder && !param.IsOptional)
                {
                    sb.Append($"{oPad}<{param.Name}...>");
                    iterations++;
                }

                if (param.IsRemainder && param.IsOptional)
                {
                    sb.Append($"{oPad}[{param.Name}...]");
                    iterations++;
                }
            }

            return sb.ToString();
        }
    }
}