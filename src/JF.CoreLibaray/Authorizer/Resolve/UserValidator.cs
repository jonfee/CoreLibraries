using JF.Security;
using Newtonsoft.Json;
using System;

namespace JF.Authorizer.Resolve
{
    /// <summary>
    /// 用户信息解析器
    /// </summary>
    internal sealed class UserValidator : TokenValidator
    {
        public override bool TryResolve(TokenResolveContext context)
        {
            try
            {
                var realPrivateKey = Tools.PrivateKeyResolver(context.Agent.Code, context.Agent.Sercert);
                var jsonData = context.Agent.Ciphertext.DecryptFor(context.PublicKey, realPrivateKey);
                var user = JsonConvert.DeserializeObject<AuthUser>(jsonData);

                if (user == null) context.AddErrors("用户信息无效");

                context.User = user;
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
