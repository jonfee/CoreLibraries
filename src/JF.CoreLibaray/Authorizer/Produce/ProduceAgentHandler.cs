using JF.Security;
using Newtonsoft.Json;
using System;

namespace JF.Authorizer.Produce
{
    /// <summary>
    /// 代理者
    /// </summary>
    internal sealed class ProduceAgentHandler : ProduceHandler
    {
        /// <summary>
        /// 加工令牌。
        /// </summary>
        /// <param name="context"></param>
        public override void Processing(TokenProduceContext context)
        {
            if (context == null) return;

            context.SetAgentCode(Guid.NewGuid().ToString("N"));

            context.SetPrivateKey(Tools.GeneratePrivateKey(context.Agent.Code));

            context.SetUserCiphertext(GenerateUserCiphertext(context));

            base.Processing(context);
        }

        /// <summary>
        /// 生成用户信息密文
        /// </summary>
        private string GenerateUserCiphertext(TokenProduceContext context)
        {
            string jsonData = string.Empty;

            jsonData = JsonConvert.SerializeObject(context.User);

            // 实际私钥
            var realPrivateKey = Tools.PrivateKeyResolver(context.Agent.Code, context.Agent.Sercert);

            // 使用公钥 + 实际私钥 加密
            return jsonData.EncryptFor(context.JwtOption.SecretKey, realPrivateKey);
        }
    }
}
