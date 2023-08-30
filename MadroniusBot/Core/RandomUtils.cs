using System;
using System.Linq;

namespace MadroniusBot.Core
{
    /// <summary>
    /// Methods used to generate randomizer seeds.
    /// </summary>
    public static class RandomUtils
    {
        static DateTime s_FirstWeek = new DateTime(2023, 08, 25, 18, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Retrieves the current week number for weeklies.
        /// Week zero started on 2025-08-25.
        /// </summary>
        /// <returns>Returns a week number.</returns>
        public static int GetWeekNumber()
        {
            var elapsed = DateTime.UtcNow.Subtract(s_FirstWeek);
            //var elapsedWeeks = elapsed.Days / 7;
            var elapsedWeeks = elapsed.Minutes;
            
            return elapsedWeeks;
        }
    }
}
