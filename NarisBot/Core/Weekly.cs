using System;
using System.Collections.Generic;

namespace NarisBot.Core
{
    public interface IReadOnlyWeekly
    {
        /// <summary>Week number.</summary>
        public int WeekNumber { get; }
        /// <summary>Preset name.</summary>
        /// <summary>Leaderboard for the weekly race.</summary>
        public IReadOnlyDictionary<string, TimeSpan> Leaderboard { get; }
        /// <summary>Timestamp at which weekly seed has been created.</summary>
        public DateTime Timestamp { get; }
    }


    /// <summary>
    /// Holds information on the weekly race settings.
    /// </summary>
    public class Weekly : IReadOnlyWeekly
    {
        /// <summary>Week number.</summary>
        public int WeekNumber;
        /// <summary>Leaderboard for the weekly race.</summary>
        public Dictionary<string, TimeSpan> Leaderboard;
        /// <summary>Timestamp at which weekly seed has been created.</summary>
        public DateTime Timestamp;

        public void AddToLeaderboard(string username, TimeSpan time)
        {
            if (Leaderboard == null)
                Leaderboard = new Dictionary<string, TimeSpan>();

            if (Leaderboard.ContainsKey(username))
                Leaderboard[username] = time;
            else
                Leaderboard.Add(username, time);
        }

        /// <summary>
        /// Load new weekly parameters into weely.
        /// </summary>
        /// <param name="weekly">Weekly instance.</param>
        public void Load(Weekly weekly)
        {
            WeekNumber = weekly.WeekNumber;
            Leaderboard = weekly.Leaderboard;
            Timestamp = weekly.Timestamp;
        }

        /// <summary>
        /// Retrieves invalid weekly settings.
        /// </summary>
        public static Weekly Invalid => new Weekly
        {
            WeekNumber = -1,
            Leaderboard = null,
            Timestamp = DateTime.MinValue
        };

        public static Weekly Blank => new Weekly
        {
            WeekNumber = WeeklyUtils.GetWeekNumber(),
            Leaderboard = null,
            Timestamp = DateTime.Now
        };

        int IReadOnlyWeekly.WeekNumber => WeekNumber;
        IReadOnlyDictionary<string, TimeSpan> IReadOnlyWeekly.Leaderboard => Leaderboard;
        DateTime IReadOnlyWeekly.Timestamp => Timestamp;
    }
}
