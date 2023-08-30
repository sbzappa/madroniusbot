using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using MadroniusBot.Core;
using MadroniusBot.IO;
using MadroniusBot.Messages;

namespace MadroniusBot.Tasks
{
    public class ResetWeeklyTask
    {
        //private Task _timerTask;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public DiscordShardedClient Discord { get; set; }
        public Weekly Weekly { get; set; }
        public Config Config { get; set; }
        public TimeSpan Interval { get; set; }

        //public void Start()
        //{
        //    _timerTask = DoWorkAsync();
        //}

        public async Task StartAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await TryResetWeekly();
                    await Task.Delay(Interval, _cts.Token);
                }
            }
            catch (OperationCanceledException _)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task TryResetWeekly()
        {
            var guilds = Discord.ShardClients
                .Select(kvp => kvp.Value)
                .SelectMany(shard => shard.Guilds)
                .Select(kvp => kvp.Value);
            
            var previousWeek = Weekly.WeekNumber;
            var currentWeek = RandomUtils.GetWeekNumber();
            var backupAndResetWeekly = previousWeek != currentWeek;

            // Make a backup of the previous week's weekly and create a new
            // weekly for the current week.
            if (!backupAndResetWeekly)
                return;
               
            foreach (var guild in guilds)
            {
                await CommandUtils.RevokeAllRolesAsync(guild, new[]
                {
                    Config.WeeklyCompletedRole,
                    Config.WeeklyForfeitedRole
                });
                
                await CommandUtils.SendToChannelAsync(
                    guild,
                    Config.WeeklyChannel,
                    Display.LeaderboardEmbed(Weekly, false));
            } 
            
            // Backup weekly settings to json before overriding.
            await WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{previousWeek}.json");

            // Set weekly to blank with a fresh leaderboard.
            Weekly.Load(Weekly.Blank);
            await WeeklyIO.StoreWeeklyAsync(Weekly);
        }

        public void StopAsync()
        {
            _cts.Cancel();
        }
        
        //public async Task StopAsync()
        //{
        //    if (_timerTask is null)
        //        return;
        //    
        //    _cts.Cancel();
        //    await _timerTask;
        //    _cts.Dispose();
        //}
    }
}