using Yamashiro.Infrastructure;
using System.Threading.Tasks;
using Yamashiro.Database;
using Discord.Commands;
using MongoDB.Driver;

namespace Yamashiro.Modules
{
    [Name("Opt")]
    [Summary("Lets you opt-in or opt-out of doing events")]
    public class OptModule: YamashiroModule
    {
        [Command("opt-out")]
        [Summary("Opts you out of doing level rewards and such")]
        async Task OptOut()
        {
            var account = await Context.GetUser();
            var msg = await Context.Channel.SendMessageAsync(":pencil2: **| Opting you out...**");
            if (account.Opted)
            {
                await msg.ModifyAsync(x =>
                {
                    x.Content = ":question: **| You already have opted out, did you mean to use the `opt-in` command?**";
                });
            }
            else
            {
                var query = Builders<UserModel>.Update.Set("opted", true);
                await Context.Database.UpdateUser(Context.User.Id, query);

                await msg.ModifyAsync(x =>
                {
                    x.Content = ":ok_hand: **| I have opted you out of earning levels and participating in events. If you wanna opt-in again, use the `opt-in` command!**";
                });
            }
        }

        [Command("opt-in")]
        [Summary("Opts you in of earning levels and participating in events.")]
        async Task OptIn()
        {
            var account = await Context.GetUser();
            var msg = await Context.Channel.SendMessageAsync(":pencil2: **| Opting you in...**");
            if (!account.Opted)
            {
                await msg.ModifyAsync(x =>
                {
                    x.Content = ":question: **| You already have opted in, did you mean to use the `opt-out` command?**";
                });
            }
            else
            {
                var query = Builders<UserModel>.Update.Set("opted", false);
                await Context.Database.UpdateUser(Context.User.Id, query);

                await msg.ModifyAsync(x =>
                {
                    x.Content = ":ok_hand: **| You are now opted in to earn levels and participating in events!**";
                });
            }
        }
    }
}