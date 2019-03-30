using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace JF.SocketCore.Server
{
    /// <summary>
    /// Socket连接池
    /// </summary>
    public sealed class SocketsPool : IDisposable
    {
        #region variables

        /// <summary>
        /// 所属平台标识
        /// </summary>
        private string _platform;

        /// <summary>
        /// 是否允许匿名
        /// </summary>
        private bool _allowAnonymous;

        /// <summary>
        /// 当前管理寄存器中的通道集合
        /// </summary>
        private ConcurrentDictionary<Guid, SocketSession> _sessions;

        #endregion

        #region contructors

        /// <summary>
        /// 实例化一个<see cref="SocketsPool"/>对象
        /// </summary>
        /// <param name="platform">所属平台标识</param>
        /// <param name="allowAnonymous">是否允许匿名</param>
        public SocketsPool(string platform = "default", bool allowAnonymous = true)
        {
            this._platform = platform;
            this._allowAnonymous = allowAnonymous;
            this._sessions = new ConcurrentDictionary<Guid, SocketSession>();
        }

        #endregion

        #region properties

        /// <summary>
        /// <see cref="WebSocket"/>在线通道总数
        /// </summary>
        public int Count => _sessions.Count;

        /// <summary>
        /// 已登录账号的通道总数
        /// </summary>
        public int LoggedCount => _sessions.Values.Count(p => p.IsLogged);

        /// <summary>
        /// 索引器（通道ID）
        /// </summary>
        /// <param name="channelId">通道ID</param>
        /// <returns></returns>
        public SocketSession this[Guid channelId] => GetSocketFor(channelId);

        /// <summary>
        /// 索引器（用户ID）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<SocketSession> this[string userId] => GetSocketsFor(userId);

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

        #region public behavious

        /// <summary>
        /// 根据通道ID获取一个<see cref="SocketSession"/>对象
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public SocketSession GetSocketFor(Guid channelId)
        {
            return _sessions.ContainsKey(channelId) ? _sessions[channelId] : default(SocketSession);
        }

        /// <summary>
        /// 获取用户的所有连接通道
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<SocketSession> GetSocketsFor(string userId)
        {
            return _sessions.Values.Where(p => p.IsLogged && p.Identity.UserId == userId);
        }

        /// <summary>
        /// 获取所有<see cref="SocketSession"/>对象集合
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<Guid, SocketSession> GetAllSockets()
        {
            return _sessions;
        }

        /// <summary>
        /// 添加一个<see cref="WebSocket"/>通道，返回<see cref="SocketSession"/>对象通道ID
        /// </summary>
        /// <param name="socket"><see cref="WebSocket"/>对象</param>
        /// <param name="ip">客户端IP</param>
        /// <param name="allowAnonymous">是否允许匿名</param>
        /// <param name="isBackground">是否后台管理Socket连接</param>
        /// <returns></returns>
        public void AddSocket(WebSocket socket, string ip, bool allowAnonymous = true, bool isBackground = false)
        {
            var session = new SocketSession(this, socket, ip, allowAnonymous);

            if (_sessions.TryAdd(session.ChannelId, session))
            {
                session.ConnectedEvent += ConnectedEvent;
                session.DisConnectedEvent += DisConnectedEvent;
                session.SendedEvent += SendedEvent;
                session.ReceiveBinaryEvent += ReceiveBinaryEvent;
                session.ReceiveTextEvent += ReceiveTextEvent;

                session.Listening(isBackground);
            }
        }

        /// <summary>
        /// 查找 <see cref="SocketSession"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool TryFind(string userId, out SocketSession session)
        {
            session = default(SocketSession);

            try
            {
                if (_sessions != null)
                {
                    session = _sessions.Values.Where(p => p.IsLogged && p.Identity.UserId == userId).FirstOrDefault();
                }
            }
            catch { }

            return session != default(SocketSession);
        }

        /// <summary>
        /// 查找用户的WebSocket通道ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public bool TryFind(string userId, out Guid channelId)
        {
            channelId = default(Guid);

            try
            {
                if (_sessions != null)
                {
                    channelId = _sessions.Values.Where(p => p.IsLogged && p.Identity.UserId == userId).Select(p => p.ChannelId).FirstOrDefault();
                }
            }
            catch { }

            return channelId != default(Guid);
        }

        /// <summary>
        /// 从集合中移除并销毁<see cref="SocketSession"/>对象
        /// </summary>
        /// <param name="channelId"><see cref="WebSocket"/>通道ID</param>
        /// <param name="closeSocket">是否关闭连接</param>
        /// <returns></returns>
        public async Task RemoveAtAsync(Guid channelId, bool closeSocket = false)
        {
            if (_sessions.TryRemove(channelId, out SocketSession session))
            {
                if (closeSocket)
                {
                    await session.CloseSocketAsync(true);
                }
            }
        }

        /// <summary>
        /// 清空其他相同身份信息的<see cref="SocketSession"/>对象身份，并发送消息
        /// </summary>
        /// <param name="sourceChannelId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ClearSameUserSessionsAtAsync(Guid sourceChannelId, string message)
        {
            var session = this[sourceChannelId];

            if (session != null)
            {
                await ClearSameUserSessionsAtAsync(session, message);
            }
        }

        /// <summary>
        /// 清空其他相同身份信息的<see cref="SocketSession"/>对象身份，并发送消息
        /// </summary>
        /// <param name="sourceChannelId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ClearSameUserSessionsAtAsync(Guid sourceChannelId, byte[] message)
        {
            var session = this[sourceChannelId];

            if (session != null)
            {
                await ClearSameUserSessionsAtAsync(session, message);
            }
        }

        /// <summary>
        /// 清空其他相同身份信息的<see cref="SocketSession"/>对象身份，并发送消息
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ClearSameUserSessionsAtAsync(SocketSession source, string message)
        {
            var sockets = this._sessions.Values.Where(p => p.Identity != null && p.Identity.UserId == source.Identity.UserId && p != source);

            if (sockets != null && sockets.Count() > 0)
            {
                foreach (var socket in sockets)
                {
                    await socket.ClearIdentityAsync(message);
                }
            }
        }

        /// <summary>
        /// 清空其他相同身份信息的<see cref="SocketSession"/>对象身份，并发送消息
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ClearSameUserSessionsAtAsync(SocketSession source, byte[] message)
        {
            var sockets = this._sessions.Values.Where(p => p.Identity != null && p.Identity.UserId == source.Identity.UserId && p != source);

            if (sockets != null && sockets.Count() > 0)
            {
                foreach (var socket in sockets)
                {
                    await socket.ClearIdentityAsync(message);
                }
            }
        }

        /// <summary>
        /// 发送消息给所有<see cref="WebSocket"/>通道
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="isLogged">是否只发送给已登录账号的通道，为null时忽略此限制。</param>
        /// <returns></returns>
        public async Task SendToAllAsync(string message, bool? isLogged = null)
        {
            foreach (var session in _sessions.Values)
            {
                if (isLogged.HasValue && session.IsLogged != isLogged.Value) continue;

                if (session.Channel.State == WebSocketState.Open)
                {
                    await SendToAsync(session, message);
                }
            }
        }

        /// <summary>
        /// 发送消息给所有<see cref="WebSocket"/>通道
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="isLogged">是否只发送给已登录账号的通道，为null时忽略此限制。</param>
        /// <returns></returns>
        public async Task SendToAllAsync(byte[] message, bool? isLogged = null)
        {
            foreach (var session in _sessions.Values)
            {
                if (isLogged.HasValue && session.IsLogged != isLogged.Value) continue;

                if (session.Channel.State == WebSocketState.Open)
                {
                    await SendToAsync(session, message);
                }
            }
        }

        /// <summary>
        /// 发送消息给指定的<see cref="SocketSession"/>
        /// </summary>
        /// <param name="targetSesstion"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToAsync(SocketSession targetSesstion, string message)
        {
            if (targetSesstion == null) return;

            await SendToAsync(targetSesstion.ChannelId, message);
        }

        /// <summary>
        /// 发送消息给指定的<see cref="SocketSession"/>
        /// </summary>
        /// <param name="targetSesstion"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToAsync(SocketSession targetSesstion, byte[] message)
        {
            if (targetSesstion == null) return;

            await SendToAsync(targetSesstion.ChannelId, message);
        }

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userIds">用户ID集合</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendToAsync(IEnumerable<string> userIds, string message)
        {
            foreach (var session in _sessions.Values)
            {
                if (!session.IsLogged) continue;

                if (userIds.Contains(session.Identity.UserId))
                {
                    await SendToAsync(session.ChannelId, message);
                }
            }
        }

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToAsync(string userId, string message)
        {
            await SendToAsync(new[] { userId }, message);
        }

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userIds">用户ID集合</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendToAsync(IEnumerable<string> userIds, byte[] message)
        {
            foreach (var session in _sessions.Values)
            {
                if (!session.IsLogged) continue;

                if (userIds.Contains(session.Identity.UserId))
                {
                    await SendToAsync(session.ChannelId, message);
                }
            }
        }

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToAsync(string userId, byte[] message)
        {
            await SendToAsync(new[] { userId }, message);

        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="targetChannelIds">目标<see cref="WebSocket"/>通道ID集合</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendToAsync(IEnumerable<Guid> targetChannelIds, string message)
        {
            if (targetChannelIds == null || targetChannelIds.Count() < 1) return;

            Task[] tasks = new Task[targetChannelIds.Count()];

            for (int i = 0; i < targetChannelIds.Count(); i++)
            {
                tasks[i] = SendToAsync(targetChannelIds.ElementAt(i), message);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="targetChannelId">目标<see cref="WebSocket"/>通道ID</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendToAsync(Guid targetChannelId, string message)
        {
            var session = _sessions.ContainsKey(targetChannelId) ? _sessions[targetChannelId] : default(SocketSession);

            await session?.SendAsync(message);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="targetChannelIds">目标<see cref="WebSocket"/>通道ID集合</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendToAsync(IEnumerable<Guid> targetChannelIds, byte[] message)
        {
            if (targetChannelIds == null || targetChannelIds.Count() < 1) return;

            Task[] tasks = new Task[targetChannelIds.Count()];

            for (int i = 0; i < targetChannelIds.Count(); i++)
            {
                tasks[i] = SendToAsync(targetChannelIds.ElementAt(i), message);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="targetChannelId">目标<see cref="WebSocket"/>通道ID</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task<bool> SendToAsync(Guid targetChannelId, byte[] message)
        {
            var session = _sessions.ContainsKey(targetChannelId) ? _sessions[targetChannelId] : default(SocketSession);

            return await session?.SendAsync(message);
        }

        /// <summary>
        /// 中止连接池
        /// </summary>
        /// <returns></returns>
        public void Abort()
        {
            if (_sessions != null)
            {
                foreach (var session in _sessions.Values)
                {
                    session.Dispose();
                }
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this.Abort();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                _sessions = null;
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~SocketsPool() {
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
