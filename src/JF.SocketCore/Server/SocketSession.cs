using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JF.SocketCore.Server
{
    /// <summary>
    /// Socket连接的Session信息
    /// </summary>
    public class SocketSession : IDisposable
    {
        #region private variables

        private SocketsPool _pool;          // 当前WebSocket通道的寄存器
        private WebSocket _channel;         // 当前WebSocket通道
        private ISocketIdentity _identity;   // 当前WebSocket通道所属的用户身份信息

        #endregion

        #region contructors

        /// <summary>
        /// 实例化一个<see cref="SocketSession"/>对象
        /// </summary>
        /// <param name="pool">所属连接池</param>
        /// <param name="socket">WebSocket连接通道</param>
        /// <param name="ip">客户端IP</param>
        /// <param name="allowAnonymous">是否允许匿名</param>
        public SocketSession(SocketsPool pool, WebSocket socket, string ip, bool allowAnonymous = true)
        {
            this._pool = pool ?? throw new ArgumentNullException(nameof(pool));
            this._channel = socket ?? throw new ArgumentNullException(nameof(socket));
            this.ChannelId = Guid.NewGuid();
            this.IP = ip;
            this.AllowAnonymous = allowAnonymous;
        }

        #endregion

        #region public properties

        /// <summary>
        /// 状态
        /// </summary>
        public WebSocketState State => this.Channel.State;

        /// <summary>
        /// 通道ID
        /// </summary>
        public Guid ChannelId { get; private set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 用户身份信息
        /// </summary>
        public ISocketIdentity Identity => this._identity;

        /// <summary>
        /// 当前通道
        /// </summary>
        public WebSocket Channel => this._channel;

        /// <summary>
        /// 是否允许匿名
        /// </summary>
        public bool AllowAnonymous { get; private set; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogged => Identity != null;

        #endregion

        #region events

        /// <summary>
        /// 连接成功后事件处理程序
        /// </summary>
        public event ConnectedHandler<object> ConnectedEvent;

        /// <summary>
        /// 关闭连接后事修的处理程序
        /// </summary>
        public event DisConnectedHandler<object> DisConnectedEvent;

        /// <summary>
        /// 发送消息后事件处理程序
        /// </summary>
        public event SendedHandler SendedEvent;

        /// <summary>
        /// 接收到文件本消息时的事件处理程序
        /// </summary>
        public event ReceiveTextHandler ReceiveTextEvent;

        /// <summary>
        /// 接收到字节流消息时的事件处理程序
        /// </summary>
        public event ReceiveBinaryHandler ReceiveBinaryEvent;

        #endregion

        #region behavious

        #region internal behavious

        /// <summary>
        /// 开始侦听
        /// </summary>
        /// <param name="isBackground">是否后台运行</param>
        internal void Listening(bool isBackground = false)
        {
            // 连接成功事件处理
            ConnectedEvent?.Invoke(this, EventArgs.Empty);

            if (isBackground)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveUntilCloseAsync), CancellationToken.None);
            }
            else
            {
                ReceiveUntilCloseAsync(CancellationToken.None);
            }
        }

        #endregion

        #region private behavious

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <returns></returns>
        private void ReceiveUntilCloseAsync(object state)
        {
            CancellationToken ct = (CancellationToken)state;

            byte[] body = new byte[8192];
            var buffer = new ArraySegment<byte>(body);
            WebSocketReceiveResult result = default(WebSocketReceiveResult);

            while (this.State == WebSocketState.Open)
            {
                try
                {
                    if (ct.IsCancellationRequested) break;

                    result = this.Channel.ReceiveAsync(buffer, ct).Result;

                    if (result.MessageType != WebSocketMessageType.Close)
                    {
                        using (var ms = new MemoryStream())
                        {
                            ms.Write(buffer.Array, buffer.Offset, result.Count);

                            ms.Seek(0, SeekOrigin.Begin);

                            body = ms.ToArray();
                        }
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        ReceiveBinaryEvent?.Invoke(this, body);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        ReceiveTextEvent?.Invoke(this, Encoding.UTF8.GetString(body));
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        this.CloseSocketAsync().Wait();
                    }
                }
                catch
                {
                    this.CloseSocketAsync().Wait();
                }
            }

            this.CloseSocketAsync().Wait();
        }

        #endregion

        #region public behavious

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="disposeSession">是否销毁当前对象</param>
        public async Task CloseSocketAsync(bool disposeSession = true)
        {
            try
            {
                await this.Channel.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
            catch
            { }
            finally
            {
                DisConnectedEvent?.Invoke(this, EventArgs.Empty);

                if (disposeSession)
                {
                    Dispose();
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Channel.State != WebSocketState.Open) return false;

            var buffer = new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(message),
                                                offset: 0,
                                                count: message.Length);

            await this.Channel.SendAsync(buffer: buffer,
                                    messageType: WebSocketMessageType.Text,
                                    endOfMessage: true,
                                    cancellationToken: CancellationToken.None);

            // 发送消息成功后事件处理
            SendedEvent?.Invoke(this, Encoding.UTF8.GetBytes(message));

            return true;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(byte[] message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Channel.State != WebSocketState.Open) return false;

            var buffer = new ArraySegment<byte>(array: message,
                                                offset: 0,
                                                count: message.Length);

            await this.Channel.SendAsync(buffer: buffer,
                                    messageType: WebSocketMessageType.Binary,
                                    endOfMessage: true,
                                    cancellationToken: CancellationToken.None);

            // 发送消息成功后事件处理
            SendedEvent?.Invoke(this, message);

            return true;
        }

        /// <summary>
        /// 设置用户身份信息
        /// </summary>
        /// <param name="identity">身份信息</param>
        public void SetIdentity(ISocketIdentity identity)
        {
            this._identity = identity;
        }

        /// <summary>
        /// 清空其他相同身份信息的<see cref="SocketSession"/>对象身份，并发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ClearSameUserSesstionsAtAsync(string message)
        {
            await this._pool.ClearSameUserSessionsAtAsync(this, message);
        }

        /// <summary>
        /// 清空其他相同身份信息的<see cref="SocketSession"/>对象身份，并发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ClearSameUserSesstionsAtAsync(byte[] message)
        {
            await this._pool.ClearSameUserSessionsAtAsync(this, message);
        }

        /// <summary>
        /// 清空身份信息，并发送消息
        /// </summary>
        /// <param name="sendMessage">当发送消息为null时，忽略</param>
        public async Task ClearIdentityAsync(string sendMessage = null)
        {
            this.SetIdentity(null);

            if (!string.IsNullOrEmpty(sendMessage))
            {
                await this.SendAsync(sendMessage, CancellationToken.None);
            }
        }

        /// <summary>
        /// 清空身份信息，并发送消息
        /// </summary>
        /// <param name="sendMessage">当发送消息为null时，忽略</param>
        public async Task ClearIdentityAsync(byte[] sendMessage = null)
        {
            this.SetIdentity(null);

            if (sendMessage != null)
            {
                await this.SendAsync(sendMessage, CancellationToken.None);
            }
        }

        #endregion

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this._identity = null;
                    Channel?.Dispose();
                    this._pool.RemoveAtAsync(this.ChannelId, false).Wait();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~SocketSession() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
