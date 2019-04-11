using JF.Exceptions;
using JF.Security;
using Newtonsoft.Json;
using System;

namespace JF.Authorizer.Resolve
{
    /// <summary>
    /// <see cref="TokenAgent"/>解析器
    /// </summary>
    internal sealed class AgentValidator : TokenValidator
    {
        public override bool TryResolve(TokenResolveContext context)
        {
            try
            {
                var jsonAgent = context.Token.DecryptFor(context.Option.SecretKey);
                var agent = JsonConvert.DeserializeObject<TokenAgent>(jsonAgent);

                // TOTO 验证TokenAgent相关信息。
                if (agent == null) throw new JFAuthorizationException("令牌代理信息不存在。");

                if (agent.Code == null || agent.Code.Length != 32) throw new JFAuthorizationException("令牌代理编号无效。");

                if (agent.Sercert == null || agent.Sercert.Length != 16) throw new JFAuthorizationException("令牌代理所持的密钥无效。");

                if (context.Option.Validates.ValidateIssuer
                    && agent.Issuer.Equals(context.Option.Issuer, StringComparison.OrdinalIgnoreCase))
                    throw new JFAuthorizationException("授权无效：Issuer");

                if (context.Option.Validates.ValidateAudience
                    && agent.Audience.Equals(context.Option.Audience, StringComparison.OrdinalIgnoreCase))
                    throw new JFAuthorizationException("授权无效：Audience");

                context.Agent = agent;
            }
            catch (Exception ex)
            {
                context.AddErrors(ex.Message);
            }

            if (!context.HasError && this.Successor != null)
            {
                return this.Successor.TryResolve(context);
            }

            return !context.HasError;
        }
    }
}
