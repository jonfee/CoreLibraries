using System;
using System.Collections.Generic;
using JF.Common;

namespace JF.Authorizer
{
    /// <summary>
    /// 令牌处理上下文。
    /// </summary>
    public class TokenProvider
    {
        #region private variables

        private JwtAuthorizerOption option;
        private IJwtter jwtter;

        #endregion

        #region contructor

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="option"></param>
        public TokenProvider(JwtAuthorizerOption option)
        {
            this.option = option ?? throw new ArgumentNullException(nameof(option));

            switch (this.option.JwtStrategy)
            {
                case JwtStrategy.Bearer:
                    jwtter = new BearerJwtter();
                    break;
                case JwtStrategy.JF_Bearer:
                    jwtter = new JFBearerJwtter();
                    break;
            }
        }

        #endregion

        #region behavious

        /// <summary>
        /// 生成令牌。
        /// </summary>
        /// <param name="user">需要授权的用户</param>
        /// <param name="token">生成后的令牌。</param>
        /// <returns></returns>
        public string WriteToken(TicketUser user, out JFAgentToken token)
        {
            token = null;

            var tokenStr = jwtter?.WriteToken(user, option, out token);

            return GetTagToken(tokenStr);
        }

        /// <summary>
        /// 解析令牌。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="readTokenFunc"></param>
        /// <param name="user"></param>
        /// <param name="agentCode"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryReadToken(string token, Func<string, JFToken> readTokenFunc, out string agentCode, out TicketUser user, out List<string> errors)
        {
            agentCode = string.Empty;
            user = null;
            errors = null;

            return jwtter != null
                 ? jwtter.TryReadToken(token, option, readTokenFunc, out agentCode, out user, out errors)
                 : false;
        }

        /// <summary>
        /// 获取头部不携带令牌标识头的Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetNoTagToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return string.Empty;

            if (token.StartsWith(jwtter.JWT_TAG))
            {
                token = token.TrimStart(jwtter.JWT_TAG);
            }

            return token;
        }

        /// <summary>
        /// 获取头部携带令牌标识头的Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetTagToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return string.Empty;

            if (!token.StartsWith(jwtter.JWT_TAG))
            {
                token = $"{jwtter.JWT_TAG}{token}";
            }

            return token;
        }

        #endregion
    }
}
