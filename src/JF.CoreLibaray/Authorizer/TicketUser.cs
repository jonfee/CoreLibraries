using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace JF.Authorizer
{
    /// <summary>
    /// 授权主体用户
    /// </summary>
    public sealed class TicketUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 附件信息。
        /// 用于拓展。非必须。
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// 票据时间
        /// </summary>
        public DateTime TicketTime { get; internal set; }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="user"></param>
        public static implicit operator Claim[] (TicketUser user)
        {
            var userData = string.Empty;
            if (user.UserData != null)
            {
                userData = JsonConvert.SerializeObject(user.UserData);
            }

            var claims = new Claim[]{
                        new Claim(nameof(user.ID),user.ID??string.Empty),
                        new Claim(nameof(user.Name),user.Name??string.Empty),
                        new Claim(nameof(user.UserData),userData),
                        new Claim(nameof(user.Host),user.Host??"127.0.0.1"),
                        new Claim(nameof(user.TicketTime),user.TicketTime.ToString("yyyy-MM-dd HH:mm:ss"))
                    };

            return claims;
        }

        public static implicit operator TicketUser(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null) return null;

            return new TicketUser
            {
                ID = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == nameof(ID)).Value,
                Name = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == nameof(Name)).Value,
                UserData = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == nameof(UserData)).Value,
                Host = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == nameof(Host)).Value,
                TicketTime = DateTime.Parse(claimsPrincipal.Claims.FirstOrDefault(c => c.Type == nameof(TicketTime)).Value)
            };
        }
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

        /// <summary>
        /// Token颁发者
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Token授权使用的客户端标识
        /// </summary>
        public string Audience { get; set; }
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

        public static implicit operator JFAgentToken(JFToken token)
        {
            JFAgentToken agentToken = null;

            if (token != null)
            {
                agentToken = new JFAgentToken
                {
                    AgentCode = $"{token.Token.Substring(0, 16)}{token.Token.Substring(token.Token.Length - 17)}",
                    Token = token.Token,
                    ExpireTicks = token.ExpireTicks
                };
            }

            return agentToken;
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
