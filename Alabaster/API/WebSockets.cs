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
    public delegate void WebSocketCallback(WebSocketMessageContext context);

    internal static class WebSocketParameters
    {
        internal const int WSDataSize = 2048;
        internal const int WSHeaderSize = 256;
    }

    public readonly struct WebSocketMessageContext
    {
        public readonly WebSocketConnection Connection;
        public readonly WebSocketModule Module;
        public readonly byte[] Data;

        internal WebSocketMessageContext(WebSocketReceiveBuffer dataBuffer, WebSocketConnection connection, WebSocketModule module)
        {
            this.Data = dataBuffer.Data.ToArray();
            this.Connection = connection;
            this.Module = module;
        }

        internal WebSocketMessageContext(byte[] data, WebSocketConnection connection, WebSocketModule module)
        {
            this.Data = data;
            this.Connection = connection;
            this.Module = module;
        }        

        public override string ToString() => Encoding.UTF8.GetString(this.Data);
    }

    public sealed class WebSocketModule
    {
        private readonly WebSocketCallback[] NumberedEvents = new WebSocketCallback[256];
        private readonly ConcurrentDictionary<string, WebSocketCallback> NamedEvents = new ConcurrentDictionary<string, WebSocketCallback>(Environment.ProcessorCount, 100);
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
        internal ConcurrentDictionary<WebSocketConnection, bool> connections = new ConcurrentDictionary<WebSocketConnection, bool>(Environment.ProcessorCount, 100);        
        internal WebSocketChannel(WebSocketModule module, long id)
        {
            this.Module = module;
            this.id = id;
            this.Name = "Channel " + id;
        }

        public WebSocketConnection[] Connections => this.connections.Keys.ToArray();

        public void Send(byte eventID, byte[] data) => this.SendAsync(eventID, data).Wait();
        public void Send(string eventName, byte[] data) => this.SendAsync(eventName, data).Wait();
        public Task SendAsync(byte eventID, byte[] data) => this.SendAsyncImplementation((WebSocketConnection connection) => { connection.SendAsync(eventID, data); });
        public Task SendAsync(string eventName, byte[] data) => this.SendAsyncImplementation((WebSocketConnection connection) => { connection.SendAsync(eventName, data); });

        private Task SendAsyncImplementation(Action<WebSocketConnection> callback)
        {
            Task[] tasks = new Task[this.connections.Count];
            int i = 0;
            foreach(WebSocketConnection connection in this.connections.Keys)
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
        private ConcurrentDictionary<WebSocketChannel, bool> channels = new ConcurrentDictionary<WebSocketChannel, bool>(Environment.ProcessorCount, 100);
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

                WebSocketReceiveBuffer buffer = new WebSocketReceiveBuffer(null);
                while (ws.State == WebSocketState.Open)
                {
                    buffer.Clear();
                    this.socketResult = ws.ReceiveAsync(buffer.Data, CancellationToken.None).Result;
                    if (this.socketResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.isOpen = false;
                        Task closing = ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        module.RunDisconnectionCallback(new WebSocketMessageContext(new byte[] { }, this, module));
                        closing.Wait();
                        break;
                    }
                    WebSocketMessageContext wsm = new WebSocketMessageContext(buffer, this, module);
                    switch(buffer.Type)
                    {
                        case WebSocketReceiveBuffer.MessageType.Numbered:
                            module.RunCallback(buffer.ID, wsm);
                            break;
                        case WebSocketReceiveBuffer.MessageType.Named:
                            module.RunCallback(buffer.Name, wsm);
                            break;
                        default:
                            throw new WebSocketException("The WebSocket message was not compliant with Alabaster WebSocket protocol.");
                    }
                }             
            }
        }

        public void Join(WebSocketChannel channel)
        {
            if (this.module != channel.Module) { return; }            
            channel.connections[this] = true;
            this.channels[channel] = true;
        }
        public void Leave(WebSocketChannel channel)
        {
            channel.connections.TryRemove(this, out _);
            this.channels.TryRemove(channel, out _);
        }

        public WebSocketChannel[] Channels => this.channels.Keys.ToArray();

        public void Send(byte eventID, byte[] data) => SendAsync(eventID, data).Wait();
        public void Send(string eventName, byte[] data) => SendAsync(eventName, data).Wait();        

        public Task SendAsync(byte eventID, byte[] data)
        {
            if (data.Length > WebSocketParameters.WSDataSize) { return Task.Run(() => { }); }
            byte[] buffer = new byte[WebSocketParameters.WSDataSize + WebSocketParameters.WSHeaderSize];
            buffer[0] = 0;
            buffer[1] = eventID;
            Array.Copy(data, 0, buffer, 2, data.Length);
            return SendAsyncImplementation(buffer);
        }

        public Task SendAsync(string eventName, byte[] data)
        {
            if(eventName.Length + 1 > WebSocketParameters.WSHeaderSize) { return Task.Run(()=> { }); }
            if(data.Length > WebSocketParameters.WSDataSize) { return Task.Run(() => { }); }
            byte[] buffer = new byte[WebSocketParameters.WSDataSize + WebSocketParameters.WSHeaderSize];
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

    internal readonly ref struct WebSocketReceiveBuffer
    {        
        internal readonly byte[] RawBuffer;
        internal enum MessageType : byte { Invalid, Numbered, Named };
        internal MessageType Type =>
            (this.RawBuffer[0] == 0) ? MessageType.Numbered :
            (Array.IndexOf(this.RawBuffer, 0) != -1) ? MessageType.Named :
            MessageType.Invalid;
        internal string Name => (this.Type == MessageType.Named) ? Encoding.UTF8.GetString(RawBuffer, 0, Array.IndexOf(RawBuffer, 0)) : null;
        internal byte ID => (this.Type == MessageType.Numbered) ? this.RawBuffer[0] : throw new InvalidOperationException("An internal error has occurred in WebSocket message handling code.");
        internal void Clear() => Array.Clear(this.RawBuffer, 0, this.RawBuffer.Length);        

        internal ArraySegment<byte> Data
        {
            get
            {
                int typeOffset = 0;
                switch (this.Type)
                {
                    case MessageType.Invalid:
                        return new ArraySegment<byte>(new byte[] { });
                    case MessageType.Numbered:
                        typeOffset = 2;
                        break;
                    case MessageType.Named:
                        typeOffset = Array.IndexOf(this.RawBuffer, 0);
                        break;
                }

                int dataLength = Array.IndexOf(this.RawBuffer, 0, typeOffset);
                return (dataLength != -1) ? new ArraySegment<byte>(RawBuffer, typeOffset, RawBuffer.Length - typeOffset) : new ArraySegment<byte>(new byte[] { });
            }
        }
        internal WebSocketReceiveBuffer(object _)
        {
            this.RawBuffer = new byte[WebSocketParameters.WSHeaderSize + WebSocketParameters.WSDataSize];
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
        private static ConcurrentBag<WebSocketModule> attachedWSModules = new ConcurrentBag<WebSocketModule>();
        public static WebSocketModule[] AttachedWebSocketModules => attachedWSModules.ToArray();
        
        public static void AttachWebSocketModule(string route, WebSocketModule module)
        { 
            Get(route, (Request req) => (req.IsWebSocketRequest) ? (Response)(new WebSocketHandshake(module, req.cw.Context)) : PassThrough.Default);
            attachedWSModules.Add(module);
        }
    }
}