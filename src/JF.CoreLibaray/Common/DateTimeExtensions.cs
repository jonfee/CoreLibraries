using System;
using System.Collections;
using System.Collections.Generic;

namespace JF.Common
{
    /// <summary>
    /// 时间扩展
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 基于Unix的时间戳,最终返回总秒数。
        /// 以1970/01/01 00:00:00为时间线。
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime datetime)
        {
            long baseTimeTicks = new DateTime(1970, 1, 1).Ticks;

            return (datetime.ToUniversalTime().Ticks - baseTimeTicks) / 10000000; //秒
        }

        /// <summary>
        /// 获取指定时间所属月份的所有自然周信息。
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="mondayIsFirstDayOfWeek">星期一是否为每周第一天</param>
        /// <returns></returns>
        public static IEnumerable<MonthWeek> GetWeeks(this DateTime datetime, bool mondayIsFirstDayOfWeek = false)
        {
            return NaturalMonth.From(datetime.Year, datetime.Month, mondayIsFirstDayOfWeek).WeeksInMonth;
        }
    }
}
