using JF;
using JF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoreTestProject
{
    public class EventBusTest
    {
        [Fact]
        public void UnitTimestampTest()
        {
           JF.EventBus.Loader.Current.LoadHandlers((a) => a.FullName.StartsWith("CoreTestProject"));

            Assert.Equal(1, 1);
        }
    }
}
