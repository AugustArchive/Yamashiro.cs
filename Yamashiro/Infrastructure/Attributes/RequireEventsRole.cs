﻿using System.Threading.Tasks;
using Discord.Commands;
using System.Linq;
using Discord;
using System;

namespace Yamashiro.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequireEvents: PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext ctx, CommandInfo cmd, IServiceProvider provider)
        {
            var user = (ctx.User as IGuildUser);
            if (user == null) return Task.FromResult(PreconditionResult.FromError($"Command {cmd.Name} cannot be ran outside of an guild."));

            return user.RoleIds.Any(r => r == 652767897600524298)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("Missing Events role!"));
        }
    }
}