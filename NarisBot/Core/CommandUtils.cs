using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext;
using DSharpPlus;
using DSharpPlus.Entities;

namespace NarisBot.Core
{
    using Messages;
    using Microsoft.VisualBasic;

    public static class CommandUtils
    {
        public const string kFriendlyMessage = "This shouldn't happen! Please contact your friendly neighbourhood developers!";

        /// <summary>
        /// Check if the bot is in a direct message.
        /// Runtime-equivalent of RequireDirectMessageAttribute.
        /// </summary>
        public static bool IsDirectMessage(CommandContext ctx)
        {
            return ctx.Channel is DiscordDmChannel;
        }

        /// <summary>
        /// Check if the bot has certain permissions.
        /// Runtime-equivalent of RequireBotPermissionsAttribute.
        /// </summary>
        public static async Task<bool> HasBotPermissions(CommandContext ctx, Permissions permissions, bool ignoreDms = true)
        {
            if (ctx.Guild == null)
                return ignoreDms;

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
            if (bot == null)
                return false;

            if (bot.Id == ctx.Guild.OwnerId)
                return true;

            var botPerms = ctx.Channel.PermissionsFor(bot);

            if ((botPerms & Permissions.Administrator) != 0)
                return true;

            return (botPerms & permissions) == permissions;
        }

        /// <summary>
        /// Send a success (or fail) reaction if the bot has the react permission.
        /// The bot won't send reactions in DMs, because there it's way easier
        /// to see if a command completes.
        /// </summary>
        public static async Task SendSuccessReaction(CommandContext ctx, bool success = true)
        {
            if (IsDirectMessage(ctx))
                return;

            if (!(await HasBotPermissions(ctx, Permissions.AddReactions)))
                return;

            var emoji = DiscordEmoji.FromName(ctx.Client, success ? Display.kValidCommandEmoji : Display.kInvalidCommandEmoji);
            await ctx.Message.CreateReactionAsync(emoji);
        }

        /// <summary>
        /// Calls SendSuccessReaction(ctx, false).
        /// </summary>
        public static Task SendFailReaction(CommandContext ctx)
        {
            return SendSuccessReaction(ctx, false);
        }

        /// <summary>
        /// Make a mention without fear of making a pinging mention.
        /// </summary>
        public static async Task<string> MentionRoleWithoutPing(CommandContext ctx, DiscordRole role)
        {
            return (await MentionRoleWithoutPing(ctx, new[] {role}))[0];
        }

        /// <summary>
        /// Make mentions without fear of making pinging mentions.
        /// </summary>
        public static async Task<string[]> MentionRoleWithoutPing(CommandContext ctx, IEnumerable<DiscordRole> roles)
        {
            var hasPerms = await HasBotPermissions(ctx, Permissions.MentionEveryone);
            return roles.Select(r => r.IsMentionable || hasPerms ? $"@({r.Name})" : r.Mention).ToArray();
        }

        private static Task<IEnumerable<DiscordRole>> ConvertRoleNamesToRoles(DiscordGuild guild, IEnumerable<string> roleStrings)
        {
            var roles = guild.Roles.Values
                .Where(role => roleStrings.Contains(role.Name));

            if (!roles.Any())
            {
                var errorMessage = $"No roles matching specified search have been found in guild {guild.Name}.";
                throw new InvalidOperationException(errorMessage);
            }

            return Task.FromResult(roles);
        }

        private static async Task<IEnumerable<DiscordMember>> ConvertMemberNamesToMembers(DiscordGuild guild, IEnumerable<string> memberStrings)
        {
            var allMembers = await guild.GetAllMembersAsync();

            var members = allMembers
                .Where(member => memberStrings.Contains(member.Username));

            if (!members.Any())
            {
                var errorMessage = $"No members matching specified search have been found in guild {guild.Name}.";
                throw new InvalidOperationException(errorMessage);
            }

            return members;
        }

        /// <summary>
        /// Grants roles to current member.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles.</exception>
        public static async Task GrantRolesToSelfAsync(CommandContext ctx, IEnumerable<string> roleStrings)
        {
            if (ctx.Guild.CurrentMember.Roles.Any(clientRole =>
                    ctx.Member.Roles.All(memberRole => clientRole.Position > memberRole.Position)))
            {
                var rolesTask = ConvertRoleNamesToRoles(ctx.Guild, roleStrings);
                await GrantRolesAsync(new[] {ctx.Member}, await rolesTask);
            }
        }

        /// <summary>
        /// Grants roles to specified members.
        /// </summary>
        /// <param name="members">Array of members.</param>
        /// <param name="roles">Array of roles.</param>
        /// <returns>Returns an asynchronous task.</returns>
        static async Task GrantRolesAsync(IEnumerable<DiscordMember> members, IEnumerable<DiscordRole> roles)
        {
            var grantTasks = new List<Task>();
            foreach (var role in roles)
            {
                var tasks = members
                    .Select(member => member.GrantRoleAsync(role))
                    .ToList();

                grantTasks.AddRange(tasks);
            }

            while (grantTasks.Any())
            {
                var finishedTask = await Task.WhenAny(grantTasks);
                grantTasks.Remove(finishedTask);
                await finishedTask;
            }
        }

        /// <summary>
        /// Revokes specified roles from all members that have them.
        /// </summary>
        /// <param name="guild">Discord guild.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles.</exception>
        public static async Task RevokeAllRolesAsync(DiscordGuild guild, IEnumerable<string> roleStrings)
        {
            var allMembersTask = guild.GetAllMembersAsync();

            var roles= await ConvertRoleNamesToRoles(guild, roleStrings);
            var allMembers = await allMembersTask;

            var members = allMembers
                .Where(member => member.Roles.Any(role => roles.Contains(role)))
                .Where(member => member.Roles.All(role =>
                    guild.CurrentMember.Roles.Any(clientRole => clientRole.Position > role.Position)));

            await RevokeRolesAsync(members, roles);
        }

        /// <summary>
        /// Revokes roles from specified members.
        /// </summary>
        /// <param name="guild">Discord guild.</param>
        /// <param name="memberStrings">Array of member names.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles or no matching members.</exception>
        public static async Task RevokeRolesAsync(DiscordGuild guild, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var roles = ConvertRoleNamesToRoles(guild, roleStrings);
            var members = ConvertMemberNamesToMembers(guild, memberStrings);
            await RevokeRolesAsync(await members, await roles);
        }

        /// <summary>
        /// Revokes roles from specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="members">Array of members.</param>
        /// <param name="roles">Array of roles.</param>
        /// <returns>Returns an asynchronous task.</returns>
        public static async Task RevokeRolesAsync(IEnumerable<DiscordMember> members, IEnumerable<DiscordRole> roles)
        {
            var revokeTasks = new List<Task>();
            foreach (var role in roles)
            {
                var tasks = members
                    .Select(member => member.RevokeRoleAsync(role))
                    .ToList();

                revokeTasks.AddRange(tasks);
            }

            while (revokeTasks.Any())
            {
                var finishedTask = await Task.WhenAny(revokeTasks);
                revokeTasks.Remove(finishedTask);
                await finishedTask;
            }
        }

        public static Task SendToChannelAsync(CommandContext ctx, string channelName, DiscordEmbed embed) =>
            SendToChannelAsync(ctx, channelName, new DiscordMessageBuilder().WithEmbed(embed));

        public static Task SendToChannelAsync(CommandContext ctx, string channelName, string message) =>
            SendToChannelAsync(ctx, channelName, new DiscordMessageBuilder().WithContent(message));

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary>
        public static async Task SendToChannelAsync(CommandContext ctx, string channelName, DiscordMessageBuilder messageBuilder)
        {
            var channel = ctx.Guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {ctx.Guild.Name}.";

                await ctx.RespondAsync(
                    errorMessage + "\n" +
                    kFriendlyMessage);

                throw new InvalidOperationException(errorMessage);
            }

            await channel.SendMessageAsync(messageBuilder);
        }

        public static Task SendToChannelAsync(DiscordGuild guild, string channelName, DiscordEmbed embed) =>
            SendToChannelAsync(guild, channelName, new DiscordMessageBuilder().WithEmbed(embed));

        public static Task SendToChannelAsync(DiscordGuild guild, string channelName, string message) =>
            SendToChannelAsync(guild, channelName, new DiscordMessageBuilder().WithContent(message));

        public static async Task SendToChannelAsync(DiscordGuild guild, string channelName, DiscordMessageBuilder messageBuilder)
        {
            var channel = guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {guild.Name}.";
                throw new InvalidOperationException(errorMessage);
            }

            await channel.SendMessageAsync(messageBuilder);
        }

        /// <summary>
        /// Verifies if current channel matches specified channel.
        /// </summary>
        public static async Task<bool> ChannelExistsInGuild(CommandContext ctx, string channelName)
        {
            var channel = ctx.Guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {ctx.Guild.Name}.";

                await ctx.RespondAsync(
                    errorMessage + "\n" +
                    kFriendlyMessage);

                throw new InvalidOperationException(errorMessage);
            }

            return ctx.Channel.Equals(channel);
        }

        /// <summary>
        /// Verifies whether member has a role that is among permitted roles for this command.
        /// </summary>
        public static async Task<bool> MemberHasPermittedRole(CommandContext ctx, IEnumerable<string> permittedRoles)
        {
            if (permittedRoles == null)
                return true;

            // Safety measure to avoid potential misuses of this command. May be revisited in the future.
            if (!ctx.Member.Roles.Any(role => permittedRoles.Contains(role.Name)))
            {
                var guildRoles = ctx.Guild.Roles
                    .Where(role => permittedRoles.Contains(role.Value.Name));

                await ctx.RespondAsync(
                    "Insufficient privileges to execute this command.\n" +
                    "This command is only available to the following roles:\n" +
                    String.Join(
                        ", ",
                        await MentionRoleWithoutPing(ctx, guildRoles.Select(r => r.Value).ToArray())
                    )
                );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a username to a user mention string if the user exists in the guild.
        /// </summary>
        /// <param name="guild">Discord guild</param>
        /// <param name="username">Username</param>
        /// <param name="mention">User mention</param>
        /// <returns>True if user exists in guild, false otherwise.</returns>
        public static bool UsernameToUserMention(IEnumerable<DiscordMember> members, string username, out string mention)
        {
            mention = String.Empty;

            var member = members.FirstOrDefault(member => member.Username.Equals(username));
            if (member == null)
                return false;

            mention = member.Mention;
            return true;
        }

        private static IEnumerable<string> Split(this string str,
            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        private static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        /// <summary>
        /// Converts a positive integer to an ordinal number string.
        /// </summary>
        public static string IntegerToOrdinal(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException("Integer must be positive");

            switch (n % 100)
            {
                case 11:
                case 12:
                case 13:
                    return $"{n}th";
            }

            switch (n % 10)
            {
                case 1 : return $"{n}st";
                case 2 : return $"{n}nd";
                case 3 : return $"{n}rd";
                default: return $"{n}th";
            }
        }
    }
}
