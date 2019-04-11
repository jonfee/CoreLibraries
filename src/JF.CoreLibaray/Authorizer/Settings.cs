namespace JF.Authorizer
{
    /// <summary>
    /// 配置集
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// 默认公钥。
        /// </summary>
        public const string DEFAULT_PUBLIC_KEY = "JONFEE88";

        /// <summary>
        /// 默认过期时间（单位：分钟）
        /// </summary>
        public const int DEFAULT_EXPIRE_MINUTES = 60;

        /// <summary>
        /// 默认Token颁发者
        /// </summary>
        public const string DEFAULT_ISSUER = "Jonfee_JWT_Bearer";

        /// <summary>
        /// 默认Token授权使用客户端标识
        /// </summary>
        public const string DEFAULT_AUDIENCE = "Jonfee_Audience";
    }
}
