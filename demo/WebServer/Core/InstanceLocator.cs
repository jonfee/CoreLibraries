using JF.SocketCore.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer.Core
{
    public class InstanceLocator
    {
        private static object locker = new object();
        private static SocketsPool socketsPool;

        public static SocketsPool SocketsPool
        {
            get
            {
                if (socketsPool == null)
                {
                    lock (locker)
                    {
                        if (socketsPool == null)
                        {
                            socketsPool = new SocketsPool("test", false);

                            socketsPool.ConnectedEvent += (session, e) =>
                            {
                                session.SendAsync("connected").Wait();
                            };

                            socketsPool.DisConnectedEvent += (session, e) =>
                            {
                                var state = session.Channel.State;
                            };

                            socketsPool.SendedEvent += (session, message) =>
                            {
                                //session.SendAsync(message).Wait();
                            };

                            socketsPool.ReceiveBinaryEvent += (session, message) =>
                            {
                                session.SendAsync(message).Wait();
                            };

                            socketsPool.ReceiveTextEvent += (session, message) =>
                            {
                                session.SendAsync(message).Wait();
                            };
                        }
                    }
                }

                return socketsPool;
            }
        }
    }
}
