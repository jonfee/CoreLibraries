namespace JF.SocketCore.Server
{
    /// <summary>
    /// 发送消息后事件处理委托
    /// </summary>
    /// <param name="session"></param>
    /// <param name="message"></param>
    public delegate void SendedHandler(SocketSession session, byte[] message);

    /// <summary>
    /// 连接成功后事件处理委托
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="session"></param>
    /// <param name="args"></param>
    public delegate void ConnectedHandler<TEventArgs>(SocketSession session, TEventArgs args);

    /// <summary>
    /// 断开连接后事件处理委托
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="session"></param>
    /// <param name="args"></param>
    public delegate void DisConnectedHandler<TEventArgs>(SocketSession session, TEventArgs args);

    /// <summary>
    /// 接收到文件本消息时的事件处理程序
    /// </summary>
    /// <param name="session"></param>
    /// <param name="message"></param>
    public delegate void ReceiveTextHandler(SocketSession session, string message);

    /// <summary>
    /// 接收到字节流消息时的事件处理程序
    /// </summary>
    /// <param name="session"></param>
    /// <param name="message"></param>
    public delegate void ReceiveBinaryHandler(SocketSession session, byte[] message);
}
