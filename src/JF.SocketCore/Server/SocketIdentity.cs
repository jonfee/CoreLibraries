namespace JF.SocketCore.Server
{
    /// <summary>
    /// WebSocet通道的用户身份信息 - 抽象类
    /// </summary>
    public interface ISocketIdentity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        string UserId { get; set; }
    }
}
