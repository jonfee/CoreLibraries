namespace JF.WebAPIExtensions
{
    /// <summary>
    /// 配置
    /// </summary>
    internal sealed class Settings
    {
        /// <summary>
        /// 默认授权方案在HTTP请求Headers中的名称。
        /// </summary>
        public const string DEFAULT_TOKEN_HEADER_WITH = "JF_Authorizer";

        /// <summary>
        /// 登录成功后登录信息在HTTP请求上下文Itemss中存储的名称。
        /// </summary>
        public const string HTTPCONTEXT_ITEMNAME_WITH_AUTHUSER = "JF_AuthUser";

        /// <summary>
        /// 解析Token时的错误信息在HTTP请求上下文Itemss中存储的名称。
        /// </summary>
        public const string HTTPCONTEXT_ITEMNAME_WITH_TOKENERRORS = "JF_AuthErrors";

        /// <summary>
        /// 默认过期时间（单位：分钟）
        /// </summary>
        public const int DEFAULT_EXPIRE_MINUTES = 60;
    }
}
