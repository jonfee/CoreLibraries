using JF.Authorizer.Produce;
using JF.Authorizer.Resolve;
using JF.Common;
using System;
using System.Collections.Generic;

namespace JF.Authorizer
{
    /// <summary>
    /// 基于自定义的JWT处理器
    /// </summary>
    internal sealed class JFBearerJwtter : IJwtter
    {
        public string JWT_TAG { get; } = "JBearer ";

        /// <summary>
        /// 读出Token信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="option"></param>
        /// <param name="readTokenFunc"></param>
        /// <param name="agentCode"></param>
        /// <param name="user"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryReadToken(string token, JfJwtOption option, Func<string, JFToken> readTokenFunc, out string agentCode, out TicketUser user, out List<string> errors)
        {
            agentCode = null;
            user = null;
            errors = null;


            if (token.StartsWith(JWT_TAG))
            {
                token = token.TrimStart(JWT_TAG);
            }

            try
            {
                using (var context = new TokenResolveContext(token, option, readTokenFunc))
                {
                    if (context.TryResolve(out user))
                    {
                        agentCode = context.Agent.Code;
                    }
                    else
                    {
                        errors = context.Errors;
                    }
                }
            }
            catch (Exception ex)
            {
                if (errors == null) errors = new List<string>();
                errors.Add(ex.Message);

                user = null;
            }

            return user != null;
        }

        /// <summary>
        /// 输出Token
        /// </summary>
        /// <param name="user"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public string WriteToken(TicketUser user, JfJwtOption option, out JFAgentToken token)
        {
            using (var context = new TokenProduceContext(user, option))
            {
                var jfToken = context.GenerateToken();
                var agentCode = context.Agent.Code;

                token = new JFAgentToken
                {
                    AgentCode = agentCode,
                    Token = jfToken.Token,
                    ExpireTicks = jfToken.ExpireTicks
                };
            }

            return token.Token;
        }

        /// <summary>
        /// 读出Token信息，仅支持从接口调用。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="option"></param>
        /// <param name="readTokenFunc"></param>
        /// <param name="agentCode"></param>
        /// <param name="user"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        bool IJwtter.TryReadToken(string token, JwtAuthorizerOption option, Func<string, JFToken> readTokenFunc, out string agentCode, out TicketUser user, out List<string> errors)
        {
            return TryReadToken(token, option, readTokenFunc, out agentCode, out user, out errors);
        }

        /// <summary>
        /// 输出Token，仅支持从接口调用。
        /// </summary>
        /// <param name="user"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        string IJwtter.WriteToken(TicketUser user, JwtAuthorizerOption option, out JFAgentToken token)
        {
            return WriteToken(user, option, out token);
        }
    }
}
