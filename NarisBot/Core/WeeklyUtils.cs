using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NarisBot.Core
{
    /// <summary>
    /// Methods used to generate randomizer seeds.
    /// </summary>
    public static class WeeklyUtils
    {
        static readonly DateTime s_FirstWeek = new DateTime(2023, 08, 25, 18, 0, 0, DateTimeKind.Utc);
        static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromDays(7.0);
        //static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromMinutes(1.0);

        /// <summary>
        /// Retrieves the duration until next weekly reset.
        /// </summary>
        /// <returns>Returns a TimeSpan duration until next weekly reset.</returns>
        public static TimeSpan GetRemainingWeeklyDuration(int weekNumber)
        {
            var currentWeek = GetWeek(weekNumber);
            var elapsed = DateTime.UtcNow.Subtract(currentWeek);
            var divide = elapsed.Divide(s_WeeklyDuration);

            if (divide >= 1.0)
                return TimeSpan.Zero;

            return s_WeeklyDuration - elapsed;
        }

        /// <summary>
        /// Retrieves the current week number for weeklies.
        /// Week zero started on 2023-08-25.
        /// </summary>
        /// <returns>Returns a week number.</returns>
        public static int GetWeekNumber()
        {
            var elapsed = DateTime.UtcNow.Subtract(s_FirstWeek);
            var divide = elapsed.Divide(s_WeeklyDuration);

            return (int)Math.Truncate(divide);
        }

        /// <summary>
        /// Retrieves the current week (starting date) for
        /// the current weekly.
        /// </summary>
        /// <returns>Returns a DateTime time stamp.</returns>
        public static DateTime GetWeek() =>
            GetWeek(GetWeekNumber());

        /// <summary>
        /// Retrieves the week (starting date) for
        /// specified week number.
        /// </summary>
        /// <returns>Returns a DateTime time stamp.</returns>
        public static DateTime GetWeek(int weekNumber)
        {
            var elapsed = s_WeeklyDuration.Multiply(weekNumber);
            return s_FirstWeek.Add(elapsed);
        }

        /// <summary>
        /// Tests whether a week descriptor is valid.
        /// </summary>
        /// <param name="weekDescriptor">Week descriptor. Must be in the YYYY-WW format.</param>
        /// <returns>Returns true if the week descriptor is valid. False otherwise.</returns>
        public static bool IsValidWeekDescriptor(string weekDescriptor)
        {
            var regex = new Regex("^(?<year>[0-9]{4})-(?<week>[0-9]{2})$");
            return regex.IsMatch(weekDescriptor);
        }

        /// <summary>
        /// Gets nicely printed weekly descriptor for a
        /// specified week number.
        /// </summary>
        /// <param name="weekNumber">Week number.</param>
        /// <returns>Returns a week descriptor.</returns>
        public static string GetWeekDescriptor() =>
            GetWeekDescriptor(GetWeekNumber());

        /// <summary>
        /// Gets nicely printed weekly descriptor for a
        /// specified week number.
        /// </summary>
        /// <param name="weekNumber">Week number.</param>
        /// <returns>Returns a week descriptor.</returns>
        public static string GetWeekDescriptor(int weekNumber) =>
            GetWeekDescriptor(GetWeek(weekNumber));

        /// <summary>
        /// Gets nicely printed weekly descriptor for a
        /// specified date.
        /// </summary>
        /// <param name="weekNumber">Week number.</param>
        /// <returns>Returns a week descriptor.</returns>
        public static string GetWeekDescriptor(DateTime dateTime)
        {
            var year = dateTime.Year;
            var weekNumberInCurrentYear = ISOWeek.GetWeekOfYear(dateTime);

            return $"{year}-{weekNumberInCurrentYear}";
        }
    }
}
