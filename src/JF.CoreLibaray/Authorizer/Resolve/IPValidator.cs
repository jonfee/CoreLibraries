using JF.Exceptions;
using System;

namespace JF.Authorizer.Resolve
{
    internal sealed class IPValidator : TokenValidator
    {
        public override bool TryResolve(TokenResolveContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(context.CurrentCheckIP) && !string.IsNullOrEmpty(context.Agent.IP))
                {
                    if (!context.CurrentCheckIP.Equals(context.Agent.IP)) throw new JFValidateException($"令牌未授权给IP:{context.CurrentCheckIP}使用。");
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
