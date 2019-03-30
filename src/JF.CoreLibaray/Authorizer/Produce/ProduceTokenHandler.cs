using JF.Security;
using Newtonsoft.Json;

namespace JF.Authorizer.Produce
{
    internal sealed class ProduceTokenHandler : ProduceHandler
    {
        public override void Processing(TokenProduceContext context)
        {
            var jsonData = JsonConvert.SerializeObject(context.Agent);

            // 公钥加密
            string token = jsonData.EncryptFor(context.PublicKey);

            context.SetToken(token);

            base.Processing(context);
        }
    }
}
