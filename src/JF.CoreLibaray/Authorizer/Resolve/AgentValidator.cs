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
                var jsonAgent = context.Token.DecryptFor(context.PublicKey);
                var agent = JsonConvert.DeserializeObject<TokenAgent>(jsonAgent);

                // TOTO 验证TokenAgent相关信息。
                if (agent == null) throw new JFValidateException("令牌代理信息不存在。");

                if (agent.Code == null || agent.Code.Length != 32) throw new JFValidateException("令牌代理编号无效。");

                if (agent.Sercert == null || agent.Sercert.Length != 16) throw new JFValidateException("令牌代理所持的密钥无效。");

                context.Agent = agent;
            }
            catch (Exception ex)
            {
                context.AddErrors(ex.Message);
            }

            if (!context.HasError && this.Successor!=null)
            {
                return this.Successor.TryResolve(context);
            }

            return !context.HasError;
        }
    }
}
