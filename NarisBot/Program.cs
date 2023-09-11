using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using NarisBot.Tasks;

namespace NarisBot
{
    using Core;
    using IO;

    internal class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var configTask = ConfigIO.LoadConfigAsync();
            var weeklyTask = WeeklyIO.LoadWeeklyAsync();

            var config = await configTask;

            var discord = new DiscordShardedClient(new DiscordConfiguration()
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents
            });

            var weekly = await weeklyTask;

            var services = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(weekly)
                .AddSingleton<IReadOnlyWeekly>(_ => weekly)
                .BuildServiceProvider();

            // Test for network before connecting to discord.
            var numberOfTries = 50;
            while (numberOfTries-- > 0)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                    break;

                await Task.Delay(1000);
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                discord.Logger.LogCritical("No network available! Please check your connection.");
                return;
            }

            var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { config.Prefix },
                Services = services
            });

            commands.RegisterCommands<Commands.CompletedCommandModule>();
            commands.RegisterCommands<Commands.ForfeitCommandModule>();
            commands.RegisterCommands<Commands.LeaderboardCommandModule>();

            foreach(var c in commands) {
                c.Value.CommandExecuted += CommandEvents.OnCommandExecuted;
                c.Value.CommandErrored += CommandEvents.OnCommandErrored;
            }

            var resetWeekly = new ResetWeekly
            {
                Discord = discord,
                Weekly = weekly,
                Config = config
            };
            var resetWeeklyTask = resetWeekly.StartAsync();
             
            await discord.StartAsync();
            
            await Task.Delay(-1);

            resetWeekly.StopAsync();
            await resetWeeklyTask;

        }
    }
}
