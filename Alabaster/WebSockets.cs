using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    public delegate WebSocketMessageContext WebSocketCallback(WebSocketMessageContext context);

    public sealed class WebSocketMessageContext
    {
        public readonly WebSocketConnection Connection;
        public readonly WebSocketModule Module;
        public readonly byte[] Data;

        internal WebSocketMessageContext(byte[] data, WebSocketConnection connection, WebSocketModule module)
        {
            this.Data = data;
            this.Connection = connection;
            this.Module = module;
        }

        internal WebSocketMessageContext(byte[] fullBuffer, int dataStart, int dataLength, WebSocketConnection connection, WebSocketModule module)
        {
            byte[] data = new byte[dataLength];
            Array.Copy(fullBuffer, dataStart, data, 0, dataLength);
            this.Data = data;
            this.Connection = connection;
            this.Module = module;
        }

        private WebSocketMessageContext() { }
        public override string ToString() => Encoding.UTF8.GetString(this.Data);
    }

    public sealed class WebSocketModule
    {
        internal const int WSDataSize = 2048;
        internal const int WSHeaderSize = 256;
        private WebSocketCallback[] NumberedEvents = new WebSocketCallback[256];
        private Dictionary<string, WebSocketCallback> NamedEvents = new Dictionary<string, WebSocketCallback>(100);
        private WebSocketCallback connectEvent;
        private WebSocketCallback disconnectEvent;
        public readonly WebSocketChannel MainChannel;
        private long channelCount = -1;

        public WebSocketModule() => this.MainChannel = this.CreateChannel();

        public WebSocketChannel CreateChannel() => new WebSocketChannel(this, Interlocked.Increment(ref this.channelCount));

        private static void SetupEventCallback(Action callback) => ServerThreadManager.Run(() => Util.InitExceptions(callback));

        public void NumberedEvent(byte number, WebSocketCallback callback) => SetupEventCallback(() => this.NumberedEvents[number] = callback );
        public void NamedEvent(string name, WebSocketCallback callback) => SetupEventCallback(() => this.NamedEvents[name] = callback );
        public void ConnectionEvent(WebSocketCallback callback) => SetupEventCallback(() => this.connectEvent = callback );
        public void DisconnectionEvent(WebSocketCallback callback) => SetupEventCallback(() => this.disconnectEvent = callback );   

        internal void RunCallback(byte number, WebSocketMessageContext data) => this.NumberedEvents[number]?.Invoke(data);
        internal void RunCallback(string name, WebSocketMessageContext data) { if (this.NamedEvents.TryGetValue(name, out WebSocketCallback callback)) { callback(data); } }
        internal void RunConnectionCallback(WebSocketMessageContext data) => this.connectEvent?.Invoke(data);
        internal void RunDisconnectionCallback(WebSocketMessageContext data) => this.disconnectEvent?.Invoke(data);
     
    }

    public sealed class WebSocketChannel
    {
        public readonly WebSocketModule Module;
        private readonly long id;
        public string Name;
        internal HashSet<WebSocketConnection> connections = new HashSet<WebSocketConnection>();        
        internal WebSocketChannel(WebSocketModule module, long id)
        {
            this.Module = module;
            this.id = id;
            this.Name = "Channel " + id;
        }

        public WebSocketConnection[] Connections => this.connections.ToArray();

        public void Send(byte eventID, byte[] data) => this.SendAsync(eventID, data).Wait();
        public void Send(string eventName, byte[] data) => this.SendAsync(eventName, data).Wait();
        public Task SendAsync(byte eventID, byte[] data) => this.SendAsyncImplementation((WebSocketConnection connection) => { connection.SendAsync(eventID, data); });
        public Task SendAsync(string eventName, byte[] data) => this.SendAsyncImplementation((WebSocketConnection connection) => { connection.SendAsync(eventName, data); });

        private Task SendAsyncImplementation(Action<WebSocketConnection> callback)
        {
            Task[] tasks = new Task[this.connections.Count];
            int i = 0;
            foreach(WebSocketConnection connection in this.connections)
            {
                tasks[i] = Task.Run(()=>callback(connection));
                i++;
            }
            Task all = Task.Run(() =>
            {
                foreach (Task t in tasks)
                {
                    t.Wait();
                }
            });
            return all;
        }
    }

    public sealed class WebSocketConnection
    {
        private WebSocket socket;
        private WebSocketReceiveResult socketResult;
        private WebSocketModule module;
        private HashSet<WebSocketChannel> channels = new HashSet<WebSocketChannel>();
        private volatile Task currentSend = Task.Run(()=> { });
        private object sendLock = new object();
        private bool isOpen = false;
        
        internal WebSocketConnection(WebSocketModule module, HttpListenerContext ctx)
        {
            WebSocketContext wctx = ctx.AcceptWebSocketAsync(null).Result;
            using (WebSocket ws = wctx.WebSocket)
            {
                this.socket = ws;
                this.isOpen = true;
                this.module = module;
                this.Join(module.MainChannel);
                module.RunConnectionCallback(new WebSocketMessageContext(new byte[] { }, this, module));

                byte[] buffer = new byte[WebSocketModule.WSHeaderSize + WebSocketModule.WSDataSize];
                ArraySegment<byte> seg = new ArraySegment<byte>(buffer);

                while (ws.State == WebSocketState.Open)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    this.socketResult = ws.ReceiveAsync(seg, CancellationToken.None).Result;
                    if (this.socketResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.isOpen = false;
                        Task closing = ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        module.RunDisconnectionCallback(new WebSocketMessageContext(new byte[] { }, this, module));
                        closing.Wait();
                        break;
                    }

                    if (buffer[0] == 0) { NumberedEvent(); }
                    else { NamedEvent(); }
                }

                bool NumberedEvent()
                {
                    int dataLength = Array.IndexOf(buffer, 0, 2);
                    if (dataLength == -1) { return false; }
                    WebSocketMessageContext wsm = new WebSocketMessageContext(buffer, 2, dataLength, this, module);
                    module.RunCallback(buffer[1], wsm);
                    return true;
                }

                bool NamedEvent()
                {
                    int nameLength = Array.IndexOf(buffer, 0, 2);
                    if (nameLength > WebSocketModule.WSHeaderSize - 1 || nameLength == -1) { return false; }
                    string name = Encoding.UTF8.GetString(buffer, 0, nameLength);
                    int dataStart = nameLength + 1;
                    int dataLength = Array.IndexOf(buffer, 0, dataStart);
                    if (dataLength == -1) { return false; }
                    WebSocketMessageContext wsm = new WebSocketMessageContext(buffer, dataStart, dataLength, this, module);
                    module.RunCallback(name, wsm);
                    return true;
                }                
            }
        }

        public void Join(WebSocketChannel channel)
        {
            if (this.module != channel.Module) { return; }
            
            channel.connections.Add(this);
            this.channels.Add(channel);
        }
        public void Leave(WebSocketChannel channel)
        {
            channel.connections.Remove(this);
            this.channels.Remove(channel);
        }

        public WebSocketChannel[] Channels => this.channels.ToArray();

        public void Send(byte eventID, byte[] data) => SendAsync(eventID, data).Wait();
        public void Send(string eventName, byte[] data) => SendAsync(eventName, data).Wait();        

        public Task SendAsync(byte eventID, byte[] data)
        {
            byte[] buffer = new byte[data.Length + 2];
            buffer[0] = 0;
            buffer[1] = eventID;
            Array.Copy(data, 0, buffer, 2, data.Length);
            return SendAsyncImplementation(buffer);
        }        

        public Task SendAsync(string eventName, byte[] data)
        {
            byte[] buffer = new byte[data.Length + eventName.Length + 1];
            byte[] eventNameBytes = Encoding.UTF8.GetBytes(eventName);
            Array.Copy(eventNameBytes, buffer, eventNameBytes.Length);
            Array.Copy(data, 0, buffer, eventNameBytes.Length + 2, data.Length);
            return SendAsyncImplementation(buffer);
        }

        private Task SendAsyncImplementation(byte[] buffer)
        {
            if(!this.isOpen) { return Task.Run(() => { }); }
            return Task.Run(() =>
            {
                lock (this.sendLock)
                {
                    this.currentSend.Wait();
                    this.currentSend = Util.RunTaskWithExceptionHandler(() => this.socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, this.socketResult.EndOfMessage, CancellationToken.None).Wait());
                }
            });
        }
    }
    
    internal sealed class WebSocketHandshake : Response
    {
        public WebSocketHandshake(WebSocketModule module, HttpListenerContext ctx)
        {
            WebSocketConnection connection = new WebSocketConnection(module, ctx);
        }
    }

    public partial class Server
    {
        public static void AttachWebSocketModule(string route, WebSocketModule module)
            => Get(route, (Request req)
                => (req.IsWebSocketRequest) ? (Response)(new WebSocketHandshake(module, req.cw.Context)) : new PassThrough() );        
    }
}