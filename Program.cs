//#define DEV

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Storage;

namespace WordHookBotPlus
{
    internal class Program
    {
        static internal DiscordClient Discord;
        static internal CommandsNextExtension Commands;
        static internal Database Database;
        static internal Table<Guild> Guilds;
        static internal Table<bool> OptedOut;
#pragma warning disable
        static internal AuthDiscordBotListApi DblApi;
        static internal IDblSelfBot DblSelfBot;
#pragma warning restore
#if DEV
        public const bool Dev = true;
#else
        public const bool DEV = false;
#endif
        private static void Main()
        {
            new Program().MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
#if DEV
            const string dt = "atoken";
#else
            const string dt = "token";
#endif
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings[dt],
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            });
            Commands = Discord.UseCommandsNext(new CommandsNextConfiguration {StringPrefixes = new[] {"w+"}});
            Commands.RegisterCommands<WordHooksCommands>();
            Commands.RegisterCommands<ServerCommands>();
            Commands.RegisterCommands<MiscCommands>();

            Database = new Database("storage.st");

            OptedOut = Database.GetTable<bool>("optedout");
            Guilds = Database.GetTable<Guild>("guilds");

            Discord.MessageCreated += HandleMessage;
            Discord.GuildAvailable += e =>
            {
                Discord.Logger.Log(LogLevel.Debug, $"Guild '{e.Guild.Name}' is available",
                    DateTime.Now);
                return Task.CompletedTask;
            };


            await Discord.ConnectAsync(new DiscordActivity($" for messages | prefix w+", ActivityType.Watching));
#if !DEV
            DblApi = new AuthDiscordBotListApi(Discord.CurrentUser.Id, ConfigurationManager.AppSettings["dbltoken"]);
            DblSelfBot = await DblApi.GetMeAsync();
#endif

            await Update();

            await Task.Delay(-1);
        }

        public async Task Update()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
#if !DEV
                    await DblSelfBot.UpdateStatsAsync(Discord.Guilds.Count);
#endif
                    await Discord.UpdateStatusAsync(new DiscordActivity(
                        $"for messages in {Discord.Guilds.Count} servers | prefix w+ ", ActivityType.Watching));
                    await Task.Delay(3600000);
                }

                // ReSharper disable once FunctionNeverReturns
            });
        }

        private static async Task HandleMessage(MessageCreateEventArgs e)
        {
            if (e.Message.Content.StartsWith("w+")) return;
            if (e.Author.IsBot) return;
            if (e.Guild == null) return;
            if (OptedOut.Has(e.Author.Id.ToString())) return;

            var g = Guilds.GetOrDefault(e.Guild.Id.ToString(), new Guild(e.Guild.Id.ToString()));
            //deserialization doesn't like exclusions - probably should look into this
            if (g?.Exclusions?.Channels?.Contains(e.Channel.Id) ?? false) return;

            var alreadyMessaged = new List<ulong>();
            foreach (var s in g.Hooks.Keys)
            {
                if (!e.Message.Content.Contains(s)) continue;
                foreach (var id in g.Hooks[s])
                {
                    var mem = await e.Guild.GetMemberAsync(id);
#if DEV
                    var check = true;
#else
                    var check = id != e.Author.Id;
#endif
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!mem.PermissionsIn(e.Channel).HasPermission(Permissions.AccessChannels) || !check ||
                        alreadyMessaged.Contains(id)) continue;
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{e.Author.Username} at {DateTime.Now} in #{e.Channel.Name} in {e.Guild.Name}",
                        Description = e.Message.Content.Replace(s, "**" + s + "**")
                    };
                    embed = embed.AddField("Jump link: ",
                        $"[link](https://discordapp.com/channels/{e.Guild.Id}/{e.Channel.Id}/{e.Message.Id})");
                    await mem.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    alreadyMessaged.Add(id);
                }
            }
        }

        public static async Task<string> GetGuildName(ulong id)
        {
            var g = await Discord.GetGuildAsync(id);
            return g.Name;
        }
    }
}