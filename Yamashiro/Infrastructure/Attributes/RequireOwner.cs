using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;

namespace Yamashiro.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequireOwner: PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext ctx, CommandInfo cmd, IServiceProvider provider)
        {
            var user = (ctx.User as IGuildUser);
            if (user == null) return Task.FromResult(PreconditionResult.FromError($"Command {cmd.Name} cannot be ran outside of an guild."));

            var config = provider.GetService<IConfigurationRoot>();

            // This is prolly bad practice but I don't know how to do it any other way
            return config["discord:ownerID"] == ctx.User.Id.ToString()
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
        }
    }
}