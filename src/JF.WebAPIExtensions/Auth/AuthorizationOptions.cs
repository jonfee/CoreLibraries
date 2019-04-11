using JF.Authorizer;
using System;

namespace JF.WebAPIExtensions.Auth
{
    /// <summary>
    /// 用于授权验证相关的参数配置类
    /// </summary>
    public class AuthorizationOptions
    {
        /// <summary>
        /// 授权持票者类型，默认为<see cref="JwtStrategy.BEARER"/>
        /// </summary>
        public JwtStrategy JwtStrategy { get; set; } = JwtStrategy.Bearer;

        /// <summary>
        /// 登录授权令牌令牌在HTTP请求的Header中的名称。
        /// </summary>
        public string HttpHeaderWith { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 过期策略
        /// </summary>
        public TokenExpireMode ExpireMode { get; set; }

        /// <summary>
        /// 失效的时间（单位：分钟）
        /// </summary>
        public int ExpireMinutes { get; set; }

        /// <summary>
        /// Token颁发者
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Token授权使用的客户端标识
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// 验证器，默认不需要验证客户端主机。
        /// </summary>
        public JwtValidates Validates { get; set; } = new JwtValidates();

        /// <summary>
        /// 根据授权代理编号读取服务器端存储的令牌信息委托方法。
        /// 如未提供此委托，将按令牌自解析后的过期时间做为令牌失效的验证标准。
        /// </summary>
        public Func<string, JFToken> ReadTokenFunc { get; set; }

        /// <summary>
        /// 更新指定授权代理的令牌信息。
        /// 一般为滑动过期时，更新令牌失效时间。
        /// </summary>
        public Func<string, JFToken, bool> UpdateTokenFunc { get; set; }

        public static implicit operator JwtAuthorizerOption(AuthorizationOptions option)
        {
            if (option == null) return null;

            return new JwtAuthorizerOption
            {
                Audience = option.Audience,
                ExpireMinutes = option.ExpireMinutes,
                ExpireMode = option.ExpireMode,
                HttpHeaderWith = option.HttpHeaderWith,
                Issuer = option.Issuer,
                JwtStrategy = option.JwtStrategy,
                SecretKey = option.SecretKey,
                Validates = option.Validates
            };
        }
    }
}
