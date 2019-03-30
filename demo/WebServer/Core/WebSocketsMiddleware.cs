using JF.SocketCore.Server;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebServer.Core
{
    public class WebSocketsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SocketsPool _socketPool;

        public WebSocketsMiddleware(RequestDelegate next, SocketsPool socketPool)
        {
            _next = next;
            _socketPool = socketPool;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                CancellationToken ct = context.RequestAborted;
                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

                _socketPool.AddSocket(socket, context.Request.Host.Host, false);
            }

            await _next.Invoke(context);
        }
    }
}
