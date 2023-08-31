# Naris Bot

An Evermizer bot for Discord.

## Available commands

- `!completed <HH:MM:SS>`: Add your name to the leaderboard for the weekly race or override your time in the leaderboard with a new one. Also gain access to the spoiler channel.
- `!forfeit`: Forfeit the weekly, but gain access to the spoiler channel. WIll add you to the leaderboard as DNF.
- `!leaderboard [weekNumber]`: Display specified week's leaderboard. Without parameters, display this week's leaderboard (only if in spoiler channel).

## Running a bot instance
### Requirements

- .Net Core: https://dotnet.microsoft.com/download
- DSharpPlus: https://github.com/DSharpPlus/DSharpPlus

### Permissions

For all commands to work, the following permissions must be given to the bot on discord:

- Send Messages
- Manage Messages
- Manage Roles (only required for roles defined in `config.json`)
- Access Channels
- Add Reactions (optional, used to confirm execution of a command)

### Configuration

First, follow these guidelines to set up a discord bot account:
https://dsharpplus.github.io/articles/basics/bot_account.html

You'll then need to create a `config.json` file with the OAuth token
required for the bot to authenticate itself with Discord. Look for
`config/config_template.json` in the repository for an example, or see below:

```
{
  "prefix": "!",
  "token": "my-token-goes-here",
  "weeklyCompletedRole": "did the evermizer weekly",
  "weeklyForfeitedRole": "forfeited the evermizer weekly",
  "weeklyChannel": "weekly",
  "weeklySpoilerChannel": "weekly_spoilers"
}
```

You can copy your `config.json` in either: 
- `config/`
- `$HOME/narisbot/config/`

### Compilation and Running

To compile the bot, run `dotnet build` in the project root.

The executable will be located at `NarisBot/bin/Debug/netcoreapp3.1/NarisBot.dll`.
A daemon script for Linux is provided at `linux/narisbotd`.

You can also build in release mode by running `dotnet build -c Release`.
The executable will then be located at `NarisBot/bin/Release/netcoreapp3.1/NarisBot.dll`.
Note that the daemon assumes we have a fresh release build.
