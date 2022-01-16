using Microsoft.CodeAnalysis.CSharp.Scripting;
using Yamashiro.Infrastructure.Collections;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Yamashiro.Logging;
using Discord.WebSocket;
using Discord.Rest;
using System.Linq;
using Discord;
using System;

// CREDIT: https://github.com/Ultz/Volte/blob/v3/src/Services/EvalService.cs
namespace Yamashiro.Services
{
    public class EvaluationService
    {
        private readonly DatabaseService db;
        private readonly ReadOnlyList<string> imports = new ReadOnlyList<string>(new List<string>() { 
            // System-related
            "System.Text.RegularExpressions",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.Linq",
            "System.Text",
            "System",

            // Yamashiro-related
            "Yamashiro.Infrastructure.Collections",
            "Yamashiro.Database",
            "Yamashiro.Services",
            "Yamashiro.Logging",
            "Yamashiro.Utilities",
            "Yamashiro",

            // Discord.NET-related
            "Discord.Commands",
            "Discord"
        });

        public EvaluationService(DatabaseService _db)
        {
            db = _db;
        }

        public async Task<CodeResult> EvalAsync(YamashiroCommandContext ctx, string code)
        {
            try
            {
                if (code.StartsWith("```cs") && code.EndsWith("```"))
                {
                    code = code.Substring(5);
                    code = code.Remove(code.LastIndexOf("```", StringComparison.OrdinalIgnoreCase), 3);
                }
                return await ExecScriptAsync(ctx, code);
            }
            catch (Exception ex)
            {
                return new CodeResult(0, -1, null, ex);
            }
            finally
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
            }
        }

        private async Task<CodeResult> ExecScriptAsync(YamashiroCommandContext ctx, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return new CodeResult(-1, ex: new Exception("Code cannot be null, be empty, or whitespace!"));

            var options = ScriptOptions.Default
                .WithReferences(typeof(DiscordSocketClient).Assembly, typeof(Bootstrap).Assembly)
                .WithImports(imports);

            var script = CSharpScript.Create(code, options, typeof(EvaluationHelper));
            var timer = Stopwatch.StartNew();
            var diagnostics = script.Compile();
            timer.Stop();

            if (diagnostics.Length > 0 && diagnostics.Any(a => a.Severity == DiagnosticSeverity.Error)) return new CodeResult(timer.ElapsedMilliseconds, diagnostics: diagnostics);

            var execTimer = new Stopwatch();

            try
            {
                execTimer.Start();

                var helper = new EvaluationHelper(ctx, db);
                var result = await script.RunAsync(helper).ConfigureAwait(false);
                execTimer.Stop();

                GC.Collect();
                return new CodeResult(execTimer.ElapsedMilliseconds, timer.ElapsedMilliseconds, result.ReturnValue);
            } 
            catch(Exception ex)
            {
                return new CodeResult(-1, ex: ex);
            }
        }
    }

    public class CodeResult
    {
        /// <summary>
        /// A list of diagnostics that the script has provided
        /// </summary>
        public IEnumerable<Diagnostic> Diagnostics;

        /// <summary>
        /// The exception (if it was thrown while script was being processed)
        /// </summary>
        public Exception CodeException;

        /// <summary>
        /// The amount of time that the script was compiled
        /// </summary>
        public long CompiledElapsed;

        /// <summary>
        /// The code result itself (or null and <see cref="CodeException">the exception</see> is defined)
        /// </summary>
        public object Result;

        /// <summary>
        /// The elapsed time in milliseconds
        /// </summary>
        public long Elapsed;

        public CodeResult(long elapsed, long compiledElapsed = -1, object result = null, Exception ex = null, IEnumerable<Diagnostic> diagnostics = null)
        {
            CompiledElapsed = compiledElapsed;
            CodeException = ex;
            Diagnostics = diagnostics;
            Elapsed = elapsed;
            Result = result;
        }

        public bool IsSuccess() => CodeException == null;
    }

    public class EvaluationHelper
    {
        public YamashiroCommandContext Context { get; }
        public DatabaseService Database { get; }
        public Logger EvalLogger { get; }

        public EvaluationHelper(YamashiroCommandContext ctx, DatabaseService db)
        {
            EvalLogger = new Logger("EvaluationHelper");
            Database = db;
            Context = ctx;
        }

        public Task<RestUserMessage> ReplyAsync(string message, Embed embed = null, bool isTTS = false, RequestOptions options = null) => Context.Channel.SendMessageAsync(message, isTTS, embed, options);
        public SocketGuildUser GetUser(ulong id) => Context.Guild.GetUser(id);
        public SocketGuildUser GetUser(string username) => Context.Guild.Users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase) || (x.Nickname != null && x.Nickname.Equals(username, StringComparison.OrdinalIgnoreCase)));
        public SocketChannel GetTextChannel(ulong id) => Context.Client.GetChannel(id);
    }
}