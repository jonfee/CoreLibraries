using JF.Exceptions;
using System;

namespace JF.Authorizer.Resolve
{
    /// <summary>
    /// 失效令牌
    /// </summary>
    internal class ExpireValidator : TokenValidator
    {
        private Func<string, JFToken> readTokenFunc;

        public ExpireValidator(Func<string, JFToken> readTokenFunc = null)
        {
            this.readTokenFunc = readTokenFunc;
        }

        public override bool TryResolve(TokenResolveContext context)
        {
            try
            {
                // 从持久化中心获取令牌相关令牌，以验证令牌是否失效。
                if (this.readTokenFunc != null)
                {
                    var jfToken = this.readTokenFunc.Invoke(context.Agent.Code);

                    if (jfToken == null) throw new JFValidateException($"系统未授予代理编号为{context.Agent.Code}的令牌。");

                    if (!jfToken.Token.Equals(context.Token)) throw new JFValidateException("令牌已失效。");

                    if (jfToken.ExpireTicks < DateTime.Now.Ticks) throw new JFValidateException("令牌已失效。");

                }
                else // 否则，直接以令牌生成时的时效作为失效对照。
                {
                    if (context.Agent.ExpireTicks < DateTime.Now.Ticks) throw new JFValidateException("令牌已失效。");
                }
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
