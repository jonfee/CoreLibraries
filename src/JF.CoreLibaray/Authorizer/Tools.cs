using JF.Common;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JF.Authorizer
{
    internal static class Tools
    {
        /// <summary>
        /// 根据<paramref name="agentCode"/>生成伪私钥(密钥)。
        /// </summary>
        /// <param name="agentCode"></param>
        /// <returns></returns>
        public static string GeneratePrivateKey(string agentCode)
        {
            // 私钥的位数，即在agentCode中取索引位的数量
            int digits = 8;
            // 默认每个索引位用两个字符表示，<10的索引位前面用0补充。
            // 所以实际长度应为索引位数的两倍。
            StringBuilder keys = new StringBuilder(digits * 2);

            if (!string.IsNullOrEmpty(agentCode) && agentCode.Length == 32)
            {
                while (digits-- > 0)
                {
                    int index = new System.Random(Guid.NewGuid().GetHashCode()).Next(agentCode.Length - 1);

                    // 每个索引位用两位字符表示，不足两的前面补0
                    keys.Append(index.ToString().PadLeft(2, '0'));
                }
            }

            return keys.ToString();
        }

        /// <summary>
        /// 根据<paramref name="secret"/>伪私钥（密钥），从<paramref name="agentCode"/>中解析出真正的私钥字符串。
        /// </summary>
        /// <param name="agentCode"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string PrivateKeyResolver(string agentCode, string secret)
        {
            if (string.IsNullOrEmpty(agentCode)) return string.Empty;
            if (agentCode.Length != 32) return string.Empty;
            if (string.IsNullOrEmpty(secret)) return string.Empty;
            if (!new Regex(@"^\d{16}$").IsMatch(secret)) return string.Empty;   // 私钥长度必须是16位数字
            if (secret.Length % 2 != 0) return string.Empty;

            string keyStr = string.Empty;
            int index = 0;

            while (secret.Length > 0)
            {
                secret = secret.TryRemove(0, 2, out var idxStr);
                index = idxStr.ToInt32();

                if (index < 0 || index > agentCode.Length - 1) index = 0;

                keyStr += agentCode[index];
            }

            return keyStr.ToUpper();
        }
    }
}
