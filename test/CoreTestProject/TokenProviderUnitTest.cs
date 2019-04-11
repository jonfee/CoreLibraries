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
        public void BearerTokenProduceTest1()
        {
            var tokenUser = new TicketUser { ID = "100", Name = "jonfee", Host = "127.0.0.1", UserData = "337883612@qq.com" };

            TokenProvider tokenProvider = new TokenProvider(new JwtAuthorizerOption
            {
                Audience = "AMS_WEB",
                Issuer = "AMS_WEB",
                ExpireMinutes = 60,
                ExpireMode = TokenExpireMode.SlidingTime,
                HttpHeaderWith = "Authorization",
                JwtStrategy = JwtStrategy.Bearer,
                SecretKey = "337B672026AC4380B28ED6D8E2F4A439",
                Validates = new JwtValidates
                {
                    ValidateAudience = true,
                    ValidateHost = true,
                    ValidateIssuer = true
                }
            });

            // 生成令牌
            var secToken = tokenProvider.WriteToken(tokenUser, out var token);

            if (tokenProvider.TryReadToken(secToken, null, out var code, out var user, out var errors))
            {
                Assert.Equal(tokenUser.ID, user.ID);
            }
        }

        [Fact]
        public void JfBearerTokenProduceTest1()
        {
            var tokenUser = new TicketUser { ID = "100", Name = "jonfee", Host = "127.0.0.1", UserData = "337883612@qq.com" };

            TokenProvider tokenProvider = new TokenProvider(new JwtAuthorizerOption
            {
                Audience = "AMS_WEB",
                Issuer = "AMS_WEB",
                ExpireMinutes = 60,
                ExpireMode = TokenExpireMode.SlidingTime,
                HttpHeaderWith = "Authorization",
                JwtStrategy = JwtStrategy.JF_Bearer,
                SecretKey = "337B672026AC4380B28ED6D8E2F4A439",
                Validates = new JwtValidates
                {
                    ValidateAudience = true,
                    ValidateHost = true,
                    ValidateIssuer = true
                }
            });

            // 生成令牌
            var secToken = tokenProvider.WriteToken(tokenUser, out var token);

            if (tokenProvider.TryReadToken(secToken, null, out var code, out var user, out var errors))
            {
                Assert.Equal(tokenUser.ID, user.ID);
            }
        }
    }
}
