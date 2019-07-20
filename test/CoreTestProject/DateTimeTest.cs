using JF;
using JF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoreTestProject
{
    public class DateTimeTest
    {
        [Fact]
        public void UnitTimestampTest()
        {
            var time = DateTime.Now;

            long actual = time.ToUnixTimestamp();
            long baseTimeTicks = new DateTime(1970, 1, 1).Ticks;

            var expected = (time.ToUniversalTime().Ticks - baseTimeTicks) / 10000000; //秒

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WeeksTest1()
        {
            IEnumerable<MonthWeek> weeks = NaturalMonth.From(2019,5,true).WeeksInMonth;

            Assert.Equal(5, weeks.Count());
            Assert.Equal(5, weeks.FirstOrDefault(p => p.NO == 1).Days);
            Assert.Equal(5, weeks.FirstOrDefault(p => p.NO == 5).Days);
        }

        [Fact]
        public void WeeksTest2()
        {
            IEnumerable<MonthWeek> weeks = NaturalMonth.From(2019, 5, false).WeeksInMonth;

            Assert.Equal(5, weeks.Count());
            Assert.Equal(4, weeks.FirstOrDefault(p => p.NO == 1).Days);
            Assert.Equal(6, weeks.FirstOrDefault(p => p.NO == 5).Days);
        }
    }
}
