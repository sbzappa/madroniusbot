using System;
using System.Globalization;

namespace NarisBot.Core
{
    /// <summary>
    /// Methods used to generate randomizer seeds.
    /// </summary>
    public static class WeeklyUtils
    {
        static readonly DateTime s_FirstWeek = new DateTime(2023, 08, 25, 18, 0, 0, DateTimeKind.Utc);
        static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromDays(7.0);
        //static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromMinutes(5.0);

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
        /// Gets nicely printed weekly descriptor for a
        /// specified week number.
        /// </summary>
        /// <param name="weekNumber">Week number.</param>
        /// <returns>Returns a week descriptor.</returns>
        public static string GetWeekDescriptor(int weekNumber)
        {
            var dateTime = GetWeek(weekNumber);

            var culture = CultureInfo.InvariantCulture;

            var year = dateTime.Year;
            var weekNumberInCurrentYear = culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Friday);

            return $"{year}-{weekNumberInCurrentYear}";
        }
    }
}
