using Yamashiro.Infrastructure.Attributes;
using System.Collections.Generic;
using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Discord.Commands;
using System.Text;
using System.Linq;
using Humanizer;
using Discord;
using System;

namespace Yamashiro.Modules
{
    [Name("Events")]
    [Summary("Module to run basic events (polls (more soon))")]
    public class EventsModule: YamashiroModule
    {
        private readonly Dictionary<int, string> emojis = new Dictionary<int, string>() {
            { 0, "\u0030\u20E3" },
            { 1, "\u0031\u20E3" },
            { 2, "\u0032\u20E3" },
            { 3, "\u0033\u20E3" },
            { 4, "\u0034\u20E3" },
            { 5, "\u0035\u20E3" },
            { 6, "\u0036\u20E3" },
            { 7, "\u0037\u20E3" },
            { 8, "\u0038\u20E3" },
            { 9, "\u0039\u20E3" }
        };

        private readonly Dictionary<string, string> numEmojis = new Dictionary<string, string>() {
            { ":zero:", "\u0030\u20E3" },
            { ":one:", "\u0031\u20E3" },
            { ":two:", "\u0032\u20E3" },
            { ":three:", "\u0033\u20E3" },
            { ":four:", "\u0034\u20E3" },
            { ":five:", "\u0035\u20E3" },
            { ":six:", "\u0036\u20E3" },
            { ":seven:", "\u0037\u20E3" },
            { ":eight:", "\u0038\u20E3" },
            { ":nine:", "\u0039\u20E3" }
        };

        [RequireStaff]
        [Command("poll")]
        [Summary("Create a poll and posts it to #polls")]
        async Task Poll(string question, [Remainder] string options)
        {
            if (!options.Contains("; "))
            {
                await ReplyAsync(":x: **| Missing `;` in the options! (i.e: a; b; c; d)**");
                return;
            }

            var questions = options.Split("; ");
            if (questions.Length < 2) 
            {
                await ReplyAsync(":pencil2: **| You must include 2 answers!**");
            }

            if (questions.Length > 9)
            {
                await ReplyAsync(":pencil2: **| Due to limitations, I can't add more then 9 answers.**");
                return;
            }

            var sb = new StringBuilder();
            sb
                .AppendLine($"<@&675410635093180454> It's time for a poll! (Poll by **{Context.User.Username}#{Context.User.Discriminator}**)")
                .AppendLine()
                .AppendLine($"__**{question}?**__")
                .AppendLine();

            foreach (var item in questions)
            {
                var index = Array.FindIndex(questions, x => x == item);
                var emoji = $":{index.ToWords().ToLower()}:";

                sb.AppendLine($"{emoji} | **{item}**");
            }

            var channel = Context.Guild.GetTextChannel(533106919569883156);
            var m = await channel.SendMessageAsync(sb.ToString());
            var stop = false;
            var success = 0;

            foreach (var item in questions)
            {
                try
                {
                    var index = Array.FindIndex(questions, x => x == item);
                    var emote = emojis[index];

                    await m.AddReactionAsync(new Emoji(emote));
                    success++;
                }
                catch
                {
                    stop = true;
                    break;
                }
            }

            if (stop) await ReplyAsync($":question: **| I was unable to put emojis in the message! I have putten {success} emojis on the message!**");
            else await ReplyAsync($":pencil2: **| Your poll {question} was posted with emojis so they can react to see!**");
        }

        [RequireStaff]
        [Command("poll-results")]
        [Summary("Gets the results of the poll")]
        async Task Command_PollResults(ulong id)
        {
            var channel = Context.Guild.GetTextChannel(533106919569883156);
            var message = await channel.GetMessageAsync(id).ConfigureAwait(false);

            if (message == null)
            {
                await ReplyAsync($":x: **| No message by id `{id}` was found in `#{channel.Name}`**");
                return;
            }

            var content = message.Content.Split(Environment.NewLine.ToCharArray());
            content = content.Skip(7).ToArray();
            content = content.Where(x => x != "").ToArray();

            var list = new List<string>();
            foreach (var arr in content)
            {
                var item = arr.Split(" | ");
                if (numEmojis.TryGetValue(item[0], out string value))
                {
                    var unicode = new Emoji(value);
                    foreach (var users in message.GetReactionUsersAsync(unicode, 100).ToEnumerable())
                    {
                        var elements = users.ToArray();
                        elements = elements.Where(x => x.Id != 613530918937821339).ToArray();

                        var ending = elements.Count() == 1 ? "" : "s";
                        list.Add($"{item[1]} => **{elements.Count()} User{ending}**");
                    }
                }
                else
                {
                    list.Add($"{item[1]} => **0 Users** (Unable to find emote?)");
                }
            }

            var items = list.ToArray();
            var sb = new StringBuilder($":pencil2: **| Results**\n");
            foreach (var item in items) sb.AppendLine(item);

            await ReplyAsync(sb.ToString());
        }
    }
}
