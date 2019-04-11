using System;
using System.Collections.Generic;

namespace JF.Authorizer
{
    /// <summary>
    /// JWT授权验证者 抽象类
    /// </summary>
    internal interface IJwtter
    {
        /// <summary>
        /// JWT令牌头标识
        /// </summary>
        string JWT_TAG {get; }

        /// <summary>
        /// 生成令牌。
        /// </summary>
        /// <param name="user">需要授权的用户</param>
        /// <param name="option"></param>
        /// <param name="token">生成后的令牌。</param>
        /// <returns></returns>
        string WriteToken(TicketUser user, JwtAuthorizerOption option, out JFAgentToken token);

        /// <summary>
        /// 解析令牌。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="option"></param>
        /// <param name="readTokenFunc"></param>
        /// <param name="user"></param>
        /// <param name="agentCode"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        bool TryReadToken(string token, JwtAuthorizerOption option, Func<string, JFToken> readTokenFunc, out string agentCode, out TicketUser user,out List<string> errors);
    }
}
