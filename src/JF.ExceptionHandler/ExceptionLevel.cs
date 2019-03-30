namespace JF.ExceptionHandler
{
    /// <summary>
    /// 异常级别
    /// </summary>
    public enum ExceptionLevel
    {
        /// <summary>
        /// 默认（系统）
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 默认（自定义）
        /// </summary>
        DefaultCustom = 1,

        /// <summary>
        /// 验证异常
        /// </summary>
        Validation = 2,

        /// <summary>
        /// 授权异常
        /// </summary>
        Authority = 3,

        /// <summary>
        /// 操作权限异常
        /// </summary>
        OperationAccess = 4,

        /// <summary>
        /// 业务异常
        /// </summary>
        Business = 5
    }
}
