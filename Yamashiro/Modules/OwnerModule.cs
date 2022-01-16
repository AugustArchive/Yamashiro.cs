using System.Text.RegularExpressions;
using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Humanizer.Localisation;
using Microsoft.CodeAnalysis;
using Yamashiro.Utilities;
using System.Collections;
using Yamashiro.Services;
using System.Diagnostics;
using Yamashiro.Logging;
using System.Management;
using Discord.Commands;
using System.Threading;
using System.Text;
using System.Linq;
using Humanizer;
using Discord;
using System;

namespace Yamashiro.Modules
{
    [Name("Owner")]
    [Group("dev")]
    [Summary("Owner-related commands")]
    public class OwnerModule: YamashiroModule
    {
        private readonly EvaluationService eval;
        private readonly HttpRequest http;
        private readonly Logger logger;

        public OwnerModule(EvaluationService _eval, HttpRequest _client)
        {
            logger = new Logger("OwnerModule");
            http = _client;
            eval = _eval;
        }

        [Command("eval")]
        [Summary("Evaluates C# code and returns a value")]
        [RequireOwner]
        async Task Command_Eval([Remainder] string script)
        {
            var result = await eval.EvalAsync(Context, script);
            var stringRep = "";

            if (result.IsSuccess())
            {
                if (result.Result != null)
                {
                    switch (result.Result)
                    {
                        case string str:
                            {
                                stringRep = str;
                            }
                            break;

                        case IDictionary dict:
                            {
                                var _sb = new StringBuilder($"Dictionary {dict.GetType().Name}:");
                                foreach (var entry in dict.Keys) _sb.AppendLine($"- {entry} => {dict[entry!]}");

                                stringRep = _sb.ToString();
                            }
                            break;

                        case IEnumerable enumer:
                            {
                                var sba = new StringBuilder($"Enumerable {enumer.GetType().Name}");
                                foreach (var entry in enumer) sba.AppendLine($"- {entry}");

                                stringRep = sba.ToString();
                            }
                            break;

                        default:
                            {
                                stringRep = result.Result.ToString();
                            }
                            break;
                    }
                }
                else
                {
                    stringRep = "Nothing";
                }
            }
            else
            {
                stringRep = result.CodeException.StackTrace ?? $"Exception: {result.CodeException.Message}";
            }

            var sb = new StringBuilder("> :ok_hand: **| Results are in!**\n");

            if (result.CompiledElapsed != -1) sb.AppendLine($"**Compilation Time**: {result.CompiledElapsed.Milliseconds().Humanize(3, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Minute)}");
            if (result.Elapsed != -1) sb.AppendLine($"**Execution Time**: {result.Elapsed.Milliseconds().Humanize(3, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Minute)}");
            sb.AppendLine("```cs");

            if (result.Diagnostics != null && result.Diagnostics.Count() > 0)
            {
                var index = 0;
                foreach (var diagnostic in result.Diagnostics)
                {
                    index++;

                    var severity = diagnostic.Severity == DiagnosticSeverity.Error ? "Error" : "Warning";
                    sb.AppendLine($"// {severity} #{index}: {diagnostic.GetMessage()}");
                }

                sb.AppendLine();
            }

            sb.AppendLine(stringRep);
            sb.AppendLine("```");

            await ReplyAsync(sb.ToString());
        }

        [Command("wsl")]
        [Summary("Run Unix commands from a WSL environment")]
        [RequireOwner]
        async Task Command_WSL([Remainder] string command)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                await ReplyAsync($":pencil2: **| Operating System is not Windows! WSL is only supported in Windows 10 Pro or higher!**");
                return;
            }

            var mngObj = GetManagementObject("Win32_OperatingSystem");
            if (mngObj == null)
            {
                await ReplyAsync(":question: **| Hm, an weird error popped up. Check the console!**");
                logger.Warn("Missing management object from System.Management (line 121)");

                return;
            }

            var name = mngObj["Name"] as string;
            var winName = name.Split("|")[0];

            // TODO: idk make this better?
            if (!(winName == "Microsoft Windows 10 Pro"))
            {
                await ReplyAsync(":x: **| Operating System is not Windows 10 Pro!**");
                return;
            }

            var stopwatch = new Stopwatch();
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = @"cmd.exe"
                }
            })
            {
                process.Start();
                process.StandardInput.WriteLine($"wsl -e \"{command}\"");
                Thread.Sleep(500);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit(5000);

                var result = process.StandardOutput.ReadToEnd();
                var res = result.Split("\n");

                res = res.Skip(3).ToArray();
                res = res.Take(res.Count() - 2).ToArray();

                stopwatch.Stop();
                var sb = new StringBuilder("> :ok_hand: **| Results are in!**\n");
                sb
                    .AppendLine()
                    .AppendLine($"**Time Tooken**: {stopwatch.ElapsedMilliseconds.Milliseconds().Humanize(3, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Minute)}")
                    .AppendLine()
                    .AppendLine("```sh")
                    .AppendLine(string.Join("\n", res))
                    .AppendLine("```");

                await ReplyAsync(sb.ToString());
            }
        }

        [Command("sharex")]
        [Summary("Shows statistics of my ShareX server")]
        [RequireOwner]
        async Task Command_ShareX()
        {
            var message = await ReplyAsync(":pencil2: **| Fetching data...**");
            var (success, data) = await http.GetAsync("https://i.augu.dev");

            if (!success)
            {
                await message.ModifyAsync(m => 
                {
                    m.Content = ":question: **| Unable to fetch data, is the server offline?**";
                });
                return;
            }

            var requests = data["requests"].ToObject<int>();
            var files = data["files"].ToObject<int>();
            var sb = new StringBuilder()
                .AppendLine($"**Files this week**: {files}")
                .AppendLine($"**Requests**: {requests}")
                .ToString();

            var embed = new EmbedBuilder()
                .WithTitle("[ ShareX Statistics ]")
                .WithDescription(sb)
                .WithColor(new Color(255, 64, 160))
                .WithFooter(item =>
                {
                    item.WithText("Copyright (c) 2020 August | https://github.com/auguwu/i.augu.dev");
                });

            await message.DeleteAsync();
            await ReplyAsync(embed: embed.Build());
        }

        private ManagementObject GetManagementObject(string className)
        {
            var wmi = new ManagementClass(className);
            foreach (var obj in wmi.GetInstances())
            {
                var o = (ManagementObject)obj;
                if (o != null) return o;
            }

            return null;
        }
    }
}