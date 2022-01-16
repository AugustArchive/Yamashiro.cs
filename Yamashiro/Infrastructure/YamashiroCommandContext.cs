using System.Threading.Tasks;
using Yamashiro.Services;
using Yamashiro.Database;
using Discord.WebSocket;
using Discord.Commands;

namespace Yamashiro.Infrastructure
{
    public class YamashiroCommandContext: SocketCommandContext
    {
        public DatabaseService Database;

        public YamashiroCommandContext(DatabaseService _db, DiscordSocketClient client, SocketUserMessage message): base(client, message) {
            Database = _db;
        }

        public async Task<UserModel> GetUser() => await Database.GetUser(User.Id);
    }
}