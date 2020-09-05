using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace WordHookBotPlus
{
    public class MiscCommands : BaseCommandModule
    {
        [Command("about"), Description("About this bot.")]
        public async Task About(CommandContext ctx)
        {
            DiscordEmbedBuilder e = new DiscordEmbedBuilder().WithTitle("About this bot")
                .AddField("Author:", "bn0367#3658").WithDescription(
                    "This bot lets you add keywords to be pinged for, based off of slack's feature. Prefix w+.\nIf you don't want your messages to be processed for hooks, run w+optout. Run w+optin if you ever want back in.");
            await ctx.RespondAsync(embed: e.Build()).ConfigureAwait(false);
        }

        [Command("vote"), Description("Vote for the bot.")]
        public async Task Vote(CommandContext ctx)
        {
            DiscordEmbedBuilder b;
#if !DEV
            var check = !await Program.DblApi?.HasVoted(ctx.User.Id)!;
#else
            var check = true;
#endif
            if (check)
            {
                b = new DiscordEmbedBuilder().WithTitle("Vote for the bot!")
                    .WithDescription("Vote with [this link](https://top.gg/bot/593234355195740171/vote)");
            }
            else
            {
                b = new DiscordEmbedBuilder().WithTitle("You've already voted!").WithDescription("Try again later.");
            }

            await ctx.RespondAsync(embed: b.Build());
        }
    }
}