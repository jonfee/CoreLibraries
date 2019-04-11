namespace JF.Authorizer
{
    /// <summary>
    /// JWT持票者类型枚举
    /// </summary>
    public enum JwtStrategy
    {
        /// <summary>
        /// 通用持票者
        /// </summary>
        Bearer = 0,
        /// <summary>
        /// <see cref="JF.Authorizer"/>实现方案的持票者
        /// </summary>
        JF_Bearer = 1
    }
}
