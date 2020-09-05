using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WordHookBotPlus.Program;

namespace WordHookBotPlus
{
    public class WordHooksCommands : BaseCommandModule
    {
        [Command("add"), Description("Add a hook to the server you're currently in."), RequireGuild]
        public async Task AddHook(CommandContext ctx, [RemainingText, Description("The hook you're adding.")]
            string hook)
        {
            var g = Guilds.GetOrDefault(ctx.Guild.Id.ToString(), new Guild(ctx.Guild.Id.ToString()));
            if (!g.AddHook(hook, ctx.User.Id))
            {
                await ctx.RespondAsync("You already have that hook in this server!").ConfigureAwait(false);
                return;
            }

            Guilds[g.Id] = g;
            await ctx.RespondAsync("Successfully added hook!").ConfigureAwait(false);
        }

        [Command("list"), Description("List all of your hooks in all servers.")]
        public async Task ListHooks(CommandContext ctx)
        {
            var userHooks = new List<(ulong, string)>();
            foreach (KeyValuePair<string, Guild> g in Guilds)
            {
                var matchedHooks = new List<(ulong, string)>();
                g.Value.GetHooksForUser(ctx.User.Id).ToList()
                    .ForEach(e => matchedHooks.Add((ulong.Parse(g.Value.Id), e)));
                userHooks.AddRange(matchedHooks);
            }

            var b = $"<@{ctx.User.Id}>, your hooks: \n```";
            var hooks = new List<string>();
            foreach (var (id, value) in userHooks)
            {
                hooks.Add($"{id}: {value}");
            }

            b += string.Join("\n", hooks);
            b += " ```";
            if (userHooks.Count != 0)
                await ctx.RespondAsync(b.ToString()).ConfigureAwait(false);
            else
                await ctx.RespondAsync($"<@{ctx.User.Id}>, you have no hooks.").ConfigureAwait(false);
        }

        [Command("remove"), Description("Remove a hook by guild name and hook.")]
        public async Task RemoveHook(CommandContext ctx, [Description("The id of the guild.")] ulong guildid,
            [Description("The text of the hook. If it has spaces in the name, enclose in quotes.")]
            string hook)
        {
            var g = Guilds.GetOrDefault(ctx.Guild.Id.ToString(), new Guild(ctx.Guild.Id.ToString()));
            if (!g.RemoveHook(hook, ctx.User.Id))
            {
                await ctx.RespondAsync("Invalid hook!").ConfigureAwait(false);
                return;
            }

            Guilds[g.Id] = g;

            await ctx.RespondAsync("Successfully removed hook!");
        }

        [Command("remove"), Description("Remove a hook by name from the current server"), RequireGuild]
        public async Task SimpleRemoveHook(CommandContext ctx, [Description("The hook that you want to remove")]
            string hook)
        {
            var g = Guilds.GetOrDefault(ctx.Guild.Id.ToString(), new Guild(ctx.Guild.Id.ToString()));
            if (!g.RemoveHook(hook, ctx.User.Id))
            {
                await ctx.RespondAsync("Invalid hook!").ConfigureAwait(false);
                return;
            }

            Guilds[g.Id] = g;

            await ctx.RespondAsync("Successfully removed hook!");
        }

        [Command("optout"), Description("Opt out from having your messages trigger hooks.")]
        public async Task OptOut(CommandContext ctx)
        {
            if (OptedOut.Has(ctx.User.Id.ToString()))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, you've already opted out.");
            }
            else
            {
                OptedOut[ctx.User.Id.ToString()] = true;
                await ctx.RespondAsync(
                    $"{ctx.User.Mention}, you've successfully opted out from having your messages trigger hooks.");
            }
        }

        [Command("optin"), Description("Opt back in to having your messages trigger hooks.")]
        public async Task OptIn(CommandContext ctx)
        {
            if (OptedOut.Has(ctx.User.Id.ToString()))
            {
                OptedOut[ctx.User.Id.ToString()] = false;
                await ctx.RespondAsync(
                    $"{ctx.User.Mention}, you've successfully opted back in to having your messages trigger hooks.");
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, you haven't opted out.");
            }
        }
    }
}