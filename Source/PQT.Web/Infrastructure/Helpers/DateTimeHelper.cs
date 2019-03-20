using System;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Helpers
{
    /// <summary>
    /// Summary description for DateTimeHelper
    /// </summary>
    public static class DateTimeHelper
    {
        private const string InvalidUnixEpochErrorMessage = "Unix epoc starts January 1st, 1970";

        /// <summary>
        ///   Convert a long into a DateTime
        /// </summary>
        public static DateTime FromUnixTime(Int64 self)
        {
            var ret = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return ret.AddSeconds(self);
        }

        /// <summary>
        ///   Convert a DateTime into a long
        /// </summary>
        public static Int64 ToUnixTime(DateTime self)
        {
            if (self == DateTime.MinValue)
                return 0;

            var epoc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var delta = self - epoc;

            if (delta.TotalSeconds < 0)
                throw new ArgumentOutOfRangeException(InvalidUnixEpochErrorMessage);

            return (long)delta.TotalSeconds;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        public static int CountDays(DayOfWeek day, DateTime start, DateTime end)
        {
            TimeSpan ts = end - start;                       // Total duration
            int count = (int)Math.Floor(ts.TotalDays / 7);   // Number of whole weeks
            int remainder = (int)(ts.TotalDays % 7);         // Number of remaining days
            int sinceLastDay = (int)(end.DayOfWeek - day);   // Number of days since last [day]
            if (sinceLastDay < 0) sinceLastDay += 7;         // Adjust for negative days since last [day]

            // If the days in excess of an even week are greater than or equal to the number days since the last [day], then count this one, too.
            if (remainder >= sinceLastDay) count++;

            return count;
        }

        //public static DateTime ToUserLocalDateTime(this DateTime self)
        //{
        //    var dealerActive = CurrentUser.DealerActive();
        //    return self.AddHours(dealerActive.TimeZone);
        //}
    }
}