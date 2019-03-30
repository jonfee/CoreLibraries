namespace JF.WebAPIExtensions.Responses
{
    /// <summary>
    /// API结果编码枚举。
    /// </summary>
    public enum ApiResultCode
    {
        /// <summary>
        /// 成功。表示业务正常。
        /// </summary>
        Success = 0,
        /// <summary>
        /// 失败。表示处理中存在未知的错误。
        /// </summary>
        Failture = 1,

        /// <summary>
        /// API参数错误。表示接口接收到的参数或值存在错误。
        /// </summary>
        ApiParamterError = 10,

        /// <summary>
        /// 未授权的请求。
        /// </summary>
        NoAuthorization = 30,

        /// <summary>
        /// 操作权限不足。
        /// </summary>
        NoAccess = 31,

        /// <summary>
        /// 未找到指定资源。
        /// </summary>
        NotFound = 40,

        /// <summary>
        /// 业务处理异常。
        /// </summary>
        BussinessException = 50
    }
}
