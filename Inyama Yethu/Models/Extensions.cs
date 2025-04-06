using System;

namespace Inyama_Yethu.Models
{
    public static class Extensions
    {
        /// <summary>
        /// Safely access the Date property on a nullable DateTime
        /// </summary>
        public static DateTime? GetDate(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.Date : null;
        }

        /// <summary>
        /// Direct access to the Date property on a nullable DateTime
        /// </summary>
        public static DateTime Date(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                throw new InvalidOperationException("Cannot access Date property on null DateTime");
            
            return dateTime.Value.Date;
        }

        /// <summary>
        /// Safely access the Days property on a nullable TimeSpan
        /// </summary>
        public static int? GetDays(this TimeSpan? timeSpan)
        {
            return timeSpan.HasValue ? timeSpan.Value.Days : null;
        }

        /// <summary>
        /// Direct access to the Days property on a nullable TimeSpan
        /// </summary>
        public static int Days(this TimeSpan? timeSpan)
        {
            return timeSpan.HasValue ? timeSpan.Value.Days : 0;
        }

        /// <summary>
        /// Safely access the Hours property on a nullable TimeSpan
        /// </summary>
        public static int? GetHours(this TimeSpan? timeSpan)
        {
            return timeSpan.HasValue ? timeSpan.Value.Hours : null;
        }

        /// <summary>
        /// Safely access the Minutes property on a nullable TimeSpan
        /// </summary>
        public static int? GetMinutes(this TimeSpan? timeSpan)
        {
            return timeSpan.HasValue ? timeSpan.Value.Minutes : null;
        }

        /// <summary>
        /// Safely access the Ticks property on a nullable DateTime
        /// </summary>
        public static long? GetTicks(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.Ticks : null;
        }
    }
} 