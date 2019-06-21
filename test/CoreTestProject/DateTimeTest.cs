using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using JF.Common;

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
    }
}
