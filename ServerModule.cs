using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using static WordHookBotPlus.Program;

namespace WordHookBotPlus
{
    public class ServerCommands : BaseCommandModule
    {
        [Command("exclude"), RequireUserPermissions(Permissions.ManageChannels), RequireGuild,
         Description(
             "Exclude the channel provided, meaning that no one will get notifications from their hooks from this channel.")]
        public async Task ExcludeChannel(CommandContext ctx, [Description("The channel to exclude")]
            DiscordChannel channel)
        {
            var g = Guilds.GetOrDefault(ctx.Guild.Id.ToString(), new Guild(ctx.Guild.Id.ToString()));
            if (!g.Exclusions.AddExclusion(channel.Id))
            {
                await ctx.RespondAsync($"Channel {channel.Mention} is already excluded!").ConfigureAwait(false);
                return;
            }
            await ctx.RespondAsync($"Successfully excluded channel {channel.Mention}!").ConfigureAwait(false);
        }

        [Command("include"), RequireUserPermissions(Permissions.ManageChannels), RequireGuild,
         Description("Include a channel that was previously excluded.")]
        public async Task IncludeChannel(CommandContext ctx, [Description("The channel to be included")]
            DiscordChannel channel)
        {
            var g = Guilds.GetOrDefault(ctx.Guild.Id.ToString(), new Guild(ctx.Guild.Id.ToString()));
            if (!g.Exclusions.RemoveExclusion(channel.Id))
            {
                await ctx.RespondAsync($"Channel {channel.Mention} isn't excluded!").ConfigureAwait(false);
                return;
            }
            await ctx.RespondAsync($"Successfully included channel {channel.Mention}!").ConfigureAwait(false);
        }

        [Command("listex"), RequireUserPermissions(Permissions.ManageChannels), RequireGuild,
         Description("List all channel exclusions in this server.")]
        public async Task ListExclusions(CommandContext ctx)
        {
            var g = Guilds.GetOrDefault(ctx.Guild.Id.ToString(), new Guild(ctx.Guild.Id.ToString()));
            if (g.Exclusions.Channels.Count > 0)
                await ctx.RespondAsync(ctx.User.Mention + ", " + (await g.Exclusions.PrintExclusions()))
                    .ConfigureAwait(false);
            else
                await ctx.RespondAsync(ctx.User.Mention + ", this server has no excluded channels.");
        }
    }
}