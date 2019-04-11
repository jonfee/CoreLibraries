namespace JF.Authorizer
{
    public abstract class JwtOption
    {
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

        public JwtValidates Validates { get; set; } = new JwtValidates();
    }

    public class BearerOption : JwtOption
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public static implicit operator BearerOption(JwtAuthorizerOption option)
        {
            if (option == null) return null;

            return new BearerOption
            {
                ExpireMinutes = option.ExpireMinutes,
                Audience = option.Audience,
                ExpireMode = option.ExpireMode,
                HttpHeaderWith = option.HttpHeaderWith,
                Issuer = option.Issuer,
                SecretKey = option.SecretKey,
                Validates = option.Validates
            };
        }

        public static implicit operator JwtAuthorizerOption(BearerOption option)
        {
            if (option == null) return null;

            return new JwtAuthorizerOption
            {
                ExpireMinutes = option.ExpireMinutes,
                Audience = option.Audience,
                ExpireMode = option.ExpireMode,
                HttpHeaderWith = option.HttpHeaderWith,
                Issuer = option.Issuer,
                SecretKey = option.SecretKey,
                JwtStrategy = JwtStrategy.Bearer,
                Validates = option.Validates
            };
        }
    }

    /// <summary>
    /// <see cref="JwtStrategy.JF_BEARER"/>策略下的JWT参数
    /// </summary>
    public class JfJwtOption : JwtOption
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public static implicit operator JfJwtOption(JwtAuthorizerOption option)
        {
            if (option == null) return null;

            return new JfJwtOption
            {
                ExpireMinutes = option.ExpireMinutes,
                Audience = option.Audience,
                ExpireMode = option.ExpireMode,
                HttpHeaderWith = option.HttpHeaderWith,
                Issuer = option.Issuer,
                SecretKey = option.SecretKey,
                Validates = option.Validates
            };
        }

        public static implicit operator JwtAuthorizerOption(JfJwtOption option)
        {
            if (option == null) return null;

            return new JwtAuthorizerOption
            {
                ExpireMinutes = option.ExpireMinutes,
                Audience = option.Audience,
                ExpireMode = option.ExpireMode,
                HttpHeaderWith = option.HttpHeaderWith,
                Issuer = option.Issuer,
                SecretKey = option.SecretKey,
                JwtStrategy = JwtStrategy.JF_Bearer,
                Validates = option.Validates
            };
        }
    }

    /// <summary>
    /// JWT授权信息参数类
    /// </summary>
    public sealed class JwtAuthorizerOption
    {
        /// <summary>
        /// JWT策略
        /// </summary>
        public JwtStrategy JwtStrategy { get; set; }

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
        /// 验证器配置
        /// </summary>
        public JwtValidates Validates { get; set; } = new JwtValidates();
    }

    /// <summary>
    /// 验证器配置
    /// </summary>
    public class JwtValidates
    {
        /// <summary>
        /// 是否验证客户端主机
        /// </summary>
        public bool ValidateHost { get; set; }

        /// <summary>
        /// 是否验证Audience
        /// </summary>
        public bool ValidateAudience { get; set; }

        /// <summary>
        /// 是否验证Issuer
        /// </summary>
        public bool ValidateIssuer { get; set; }
    }
}
