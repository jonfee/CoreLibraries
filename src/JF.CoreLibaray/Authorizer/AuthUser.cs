using JF.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace JF.Authorizer
{
    /// <summary>
    /// 授权主体用户
    /// </summary>
    public sealed class AuthUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 令牌代理持有者
    /// </summary>
    public sealed class TokenAgent
    {
        /// <summary>
        /// 代理编号，固定的32位长度的字符串。
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 授权时代理所属的IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 代理密钥。
        /// 非实际私钥，此为私钥的密文（通过特殊算法加密）
        /// </summary>
        public string Sercert { get; set; }

        /// <summary>
        /// 密文。
        /// </summary>
        public string Ciphertext { get; set; }

        /// <summary>
        /// 失效策略模式。
        /// </summary>
        public int ExpireMode { get; set; }

        /// <summary>
        /// 令牌创建时间、即登录时间。
        /// </summary>
        public long CreatedTicks { get; set; }

        /// <summary>
        /// 令牌失效时间。
        /// 滑动过期策略时，忽略此值，实际过期时间以授权中心服务器管理令牌失效时间为准。
        /// </summary>
        public long ExpireTicks { get; set; }
    }

    /// <summary>
    /// 令牌信息
    /// </summary>
    public sealed class JFToken
    {
        /// <summary>
        /// 令牌字符串。
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 失效时间Ticks
        /// </summary>
        public long ExpireTicks { get; set; }

        public static implicit operator JFToken(JFAgentToken agentToken)
        {
            JFToken token = null;

            if (agentToken != null)
            {
                token = new JFToken
                {
                    Token = agentToken.Token,
                    ExpireTicks = agentToken.ExpireTicks
                };
            }

            return token;
        }
    }

    /// <summary>
    /// 带代理编号的令牌信息
    /// </summary>
    public sealed class JFAgentToken
    {
        /// <summary>
        /// 代理用户编号
        /// </summary>
        public string AgentCode { get; set; }

        /// <summary>
        /// 令牌字符串。
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 失效时间Ticks
        /// </summary>
        public long ExpireTicks { get; set; }
    }
}
