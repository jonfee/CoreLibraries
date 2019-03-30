using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class Configs
    {
        /// <summary>
        /// <see cref="WebSocketOptions"/>
        /// </summary>
        public static WebSocketOptions SocketOptions
        {
            get
            {
                return Startup.Configuration.GetSection("WebSocketOptions").Get<SocketOptions>();
            }
        }
    }

    /// <summary>
    /// SocketOptions
    /// </summary>
    public class SocketOptions
    {
        public int KeepAliveIntervalSeconds { get; set; }

        public int ReceiveBufferSize { get; set; }

        public static implicit operator WebSocketOptions(SocketOptions options)
        {
            if (options == null) return null;

            return new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(options.KeepAliveIntervalSeconds),
                ReceiveBufferSize = options.ReceiveBufferSize
            };
        }
    }
}
