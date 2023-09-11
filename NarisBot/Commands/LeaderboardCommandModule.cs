using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace NarisBot.Commands
{
    using System;
    using Core;
    using IO;
    using Messages;

    /// <summary>
    /// Implements the leaderboard command.
    /// This command is used to display the leaderboard for the weekly race.
    /// Optionally a leaderboard for previous weekly can be displayed by
    /// specifying the week number.
    /// </summary>
    public class LeaderboardCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Weekly settings.
        /// </summary>
        public IReadOnlyWeekly Weekly { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }

        /// <summary>
        /// Executes the leaderboard command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="weekDescriptor">Week descriptor. Expecting YYYY-WW format.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("leaderboard")]
        [Description("Get the weekly leaderboard.")]
        [Aliases("lb")]
        [Cooldown(15, 600, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx, string weekDescriptor = "")
        {            // Validate week descriptor
            if (!WeeklyUtils.IsValidWeekDescriptor(weekDescriptor))
            {
                await ctx.RespondAsync($"Invalid week descriptor! Expecting week with a YYYY-WW format.\n");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var currentWeekDescriptor = WeeklyUtils.GetWeekDescriptor();
            weekDescriptor = String.IsNullOrEmpty(weekDescriptor) ? currentWeekDescriptor : weekDescriptor;

            var weekly = Weekly;
            if (weekDescriptor == currentWeekDescriptor)
            {
                var success = await CommandUtils.ChannelExistsInGuild(ctx, Config.WeeklySpoilerChannel);
                if (!success)
                {
                    await ctx.RespondAsync("This week's leaderboard can only be displayed on the spoiler channel!");
                    await CommandUtils.SendFailReaction(ctx);
                    return;
                }
            }
            else
            {
                weekly = await WeeklyIO.LoadWeeklyAsync($"weekly.{weekDescriptor}.json");

                if (weekly.Leaderboard == null || weekly.Leaderboard.Count == 0)
                {
                    await ctx.RespondAsync($"No leaderboard available for week {weekDescriptor}.");
                    await CommandUtils.SendFailReaction(ctx);
                    return;
                }
            }

            await ctx.RespondAsync(Display.LeaderboardEmbed(ctx.Guild, weekly, false));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
