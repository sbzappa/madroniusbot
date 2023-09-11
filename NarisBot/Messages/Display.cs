using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using NarisBot.Core;

namespace NarisBot.Messages
{
    /// <summary>
    /// Display message to Discord.
    /// </summary>
    public static class Display
    {
        public const string kValidCommandEmoji = ":white_check_mark:";
        public const string kInvalidCommandEmoji = ":no_entry_sign:";

        public static readonly string[] kRankingEmoijs = new []
        {
            ":first_place:",
            ":second_place:",
            ":third_place:"
        };

        /// <summary>
        /// Displays the leaderboard for specified weekly.
        /// </summary>
        /// <param name="weekly">Weekly settings.</param>
        /// <param name="preventSpoilers">Hide potential spoilers.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder LeaderboardEmbed(DiscordGuild guild, IReadOnlyWeekly weekly, bool preventSpoilers)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Leaderboard",
                Color = DiscordColor.Yellow
            };

            embed
                .AddField("Weekly Seed", $"Week {WeeklyUtils.GetWeekDescriptor(weekly.WeekNumber)}");

            IEnumerable<KeyValuePair<string, TimeSpan>> leaderboard = weekly.Leaderboard;

            // To avoid giving away any ranking, avoid sorting the leaderboard when preventing spoilers.
            if (!preventSpoilers)
            {
                leaderboard = leaderboard?
                    .OrderBy(kvp => kvp.Value);
            }

            var rankStrings = String.Empty;
            var userStrings = String.Empty;
            var timeStrings = String.Empty;

            var rank = 0;
            var rankTreshold = TimeSpan.MinValue;

            if (leaderboard != null)
            {
                foreach (var entry in leaderboard)
                {
                    if (entry.Value > rankTreshold)
                    {
                        ++rank;
                        rankTreshold = entry.Value;
                    }

                    rankStrings += $"{(rank <= 3 ? kRankingEmoijs[rank - 1] : CommandUtils.IntegerToOrdinal(rank))}\n";

                    if (CommandUtils.UsernameToUserMention(guild, entry.Key, out var userMention))
                        userStrings += $"{userMention}\n";
                    else
                        userStrings += $"{entry.Key}\n";

                    timeStrings += $"{(entry.Value.Equals(TimeSpan.MaxValue) ? "DNF" : entry.Value.ToString())}\n";
                }
            }
            else
            {
                rankStrings = "\u200B";
                userStrings = "n/a";
                timeStrings = "n/a";
            }

            if (preventSpoilers)
            {
                timeStrings = Formatter.Spoiler(timeStrings);
            }

            if (!preventSpoilers)
                embed.AddField("\u200B", rankStrings, true);

            embed.AddField("User", userStrings, true);
            embed.AddField("Time", timeStrings, true);

            return embed;
        }
    }
}
