using JF.Authorizer.Produce;
using JF.Authorizer.Resolve;
using System;
using System.Collections.Generic;

namespace JF.Authorizer
{
    /// <summary>
    /// 令牌处理上下文。
    /// </summary>
    public class TokenProvider
    {
        #region private variables

        /// <summary>
        /// 公钥
        /// </summary>
        private string publicKey;

        #endregion

        #region contructor

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publicKey">公钥</param>
        public TokenProvider(string publicKey = null)
        {
            this.publicKey = publicKey ?? Settings.DEFAULT_PUBLIC_KEY;
        }

        #endregion

        #region Interface implementation

        /// <summary>
        /// 生成令牌。
        /// </summary>
        /// <param name="user">需要授权的用户</param>
        /// <param name="expireMinutes">失效时间（单位：分钟）</param>
        /// <param name="mode">失效模式，枚举<see cref="TokenExpireMode"/></param>
        /// <param name="token">生成后的令牌。</param>
        /// <returns></returns>
        public virtual bool TryProduce(AuthUser user, int expireMinutes, TokenExpireMode mode, out JFAgentToken token)
        {
            token = default(JFAgentToken);

            try
            {
                using (var context = new TokenProduceContext(user, publicKey, expireMinutes, mode))
                {
                    var t = context.Output();

                    token = new JFAgentToken
                    {
                        AgentCode = context.Agent.Code,
                        Token = t.Token,
                        ExpireTicks = t.ExpireTicks
                    };
                }
            }
            catch
            {
                // Tag: 暂不需要处理。
            }

            return token != default(JFAgentToken);
        }

        /// <summary>
        /// 解析令牌。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestCheckIP">当前请求的IP，为NULL时表示不验证IP。</param>
        /// <param name="readTokenFunc">
        /// 从持久化方案中读取<see cref="JFToken"/>信息的委托方法。
        /// 一般用于滑动过期时需要使用，绝对过期时，<paramref name="token"/>具有自验证功能。
        /// </param>
        /// <param name="user"></param>
        /// <param name="agentCode"></param>
        /// <param name="errors">解析过程中的错误消息。</param>
        /// <returns></returns>
        public virtual bool TryResolve(string token, string requestCheckIP, Func<string, JFToken> readTokenFunc, out string agentCode, out AuthUser user, out List<string> errors)
        {
            bool success = false;
            agentCode = null;
            user = null;
            errors = null;

            try
            {
                using (var context = new TokenResolveContext(token, this.publicKey, requestCheckIP, readTokenFunc))
                {
                    if (!context.TryResolve(out user))
                    {
                        errors = context.Errors;
                    }

                    agentCode = context.Agent.Code;
                    success = context.IsValid;
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        #endregion
    }
}
