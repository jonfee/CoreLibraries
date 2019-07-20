using System;
using System.Collections.Generic;

namespace JF
{
    /// <summary>
    /// 自然月
    /// </summary>
    public struct NaturalMonth
    {
        /// <summary>
        /// 星期一是否为每周第一天。
        /// True 表示每周第一天是星期一；
        /// False 表示每周第一天是星期日。
        /// </summary>
        private bool mondayIsFirstDayOfWeek;

        /// <summary>
        /// 初始化<see cref="NaturalMonth"/>结构。
        /// </summary>
        /// <param name="year">自然年份</param>
        /// <param name="month">自然月份</param>
        /// <param name="mondayIsFirstDayOfWeek">星期一是否为每周第一天</param>
        public NaturalMonth(int year, int month, bool mondayIsFirstDayOfWeek = false)
        {
            this.Year = year;
            this.Month = month;
            this.mondayIsFirstDayOfWeek = mondayIsFirstDayOfWeek;
        }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; private set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; private set; }

        /// <summary>
        /// 当前自然月中的所有周信息。
        /// </summary>
        public IEnumerable<MonthWeek> WeeksInMonth
        {
            get
            {
                return GetWeeks();
            }
        }

        /// <summary>
        /// 总共天数。
        /// </summary>
        public int Days
        {
            get
            {
                return DateTime.DaysInMonth(this.Year, this.Month);
            }
        }

        /// <summary>
        /// 根据当前时间获取自然月结构对象。
        /// </summary>
        public static NaturalMonth Now
        {
            get
            {
                var dt = DateTime.Now;

                return new NaturalMonth(dt.Year, dt.Month);
            }
        }

        /// <summary>
        /// 从指定年份和月份获得一个<see cref="NaturalMonth"/>对象实例。
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="mondayIsFirstDayOfWeek"></param>
        /// <returns></returns>
        public static NaturalMonth From(int year, int month, bool mondayIsFirstDayOfWeek = false)
        {
           return new NaturalMonth(year, month, mondayIsFirstDayOfWeek);
        }

        private IEnumerable<MonthWeek> GetWeeks()
        {
            // 结果变量
            List<MonthWeek> weeks = new List<MonthWeek>();
            // 当前月第一天是星期几？0 表示星期天，1表示星期一，2表示星期二，……6表示星期六
            int firstDayOfWeek = (int)new DateTime(this.Year, this.Month, 1).DayOfWeek;
            // 第一周的最后一天是几号？
            int endDayOfFirstWeek = 1;
            if (this.mondayIsFirstDayOfWeek)
            {
                firstDayOfWeek -= 1;
                if (firstDayOfWeek < 0) firstDayOfWeek = 6;
            }
            endDayOfFirstWeek = 7 - firstDayOfWeek;

            // 当前处理周
            int currentWeekNo = 1;
            // 当前处理周
            int lastDayOfCurrentWeek = endDayOfFirstWeek;
            // 处理第一周
            weeks.Add(new MonthWeek
            {
                NO = currentWeekNo,
                StartDay = 1,
                EndDay = lastDayOfCurrentWeek
            });
            // 处理其它周
            while (lastDayOfCurrentWeek < this.Days)
            {
                var startDay = lastDayOfCurrentWeek + 1;
                lastDayOfCurrentWeek += 7;
                if (lastDayOfCurrentWeek > this.Days) lastDayOfCurrentWeek = this.Days;

                weeks.Add(new MonthWeek
                {
                    NO = ++currentWeekNo,
                    StartDay = startDay,
                    EndDay = lastDayOfCurrentWeek
                });
            }

            return weeks;
        }
    }

    /// <summary>
    /// 智能预算-每月的自然周
    /// </summary>
    public struct MonthWeek
    {
        /// <summary>
        /// 排序号，第NO周
        /// </summary>
        public int NO { get; set; }

        /// <summary>
        /// 开始日期，如：1表示1号
        /// </summary>
        public int StartDay { get; set; }

        /// <summary>
        /// 结束日期，如：7表示7号
        /// </summary>
        public int EndDay { get; set; }

        /// <summary>
        /// 总天数
        /// </summary>
        public int Days
        {
            get
            {
                return EndDay - StartDay + 1;
            }
        }
    }
}
