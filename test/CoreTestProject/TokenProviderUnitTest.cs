using JF.Authorizer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CoreTestProject
{
    public class TokenProviderUnitTest
    {
        [Fact]
        public void TokenProduceTest1()
        {
            var tokenUser = new JF.Authorizer.AuthUser { UserID = "100", Name = "jonfee" };

            TokenProvider tokenProvider = new TokenProvider();

            // 生成令牌
            var success = tokenProvider.TryProduce(tokenUser, 60, TokenExpireMode.SlidingTime, out var agentToken);

            Assert.True(success);
        }
    }
}
