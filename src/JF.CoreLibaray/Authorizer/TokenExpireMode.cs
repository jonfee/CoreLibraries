namespace JF.Authorizer
{
    /// <summary>
    /// 令牌失效策略枚举。
    /// </summary>
    public enum TokenExpireMode
    {
        /// <summary>
        /// 绝对过期策略
        /// </summary>
        AbsoluteTime = 0,

        /// <summary>
        /// 滑动过期策略
        /// </summary>
        SlidingTime = 1
    }
}
