using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Alabaster
{
    public static partial class Logger
    {
        public static void Log(Channel channel, params Message[] messages) => ServerThreadManager.Run(() => channel.Handler(String.Join(' ', messages), new HashSet<Channel>()));
        public static void Log(params Message[] messages) => Log(DefaultLoggers.Default, messages);
        public readonly struct Message
        {
            public readonly string Content;
            public Message(string content) => this.Content = content;
            public Message(object content) : this(content.ToString()) { }
            public Message(Array array) => this.Content = "[" + String.Join(", ", array) + "]";
            public static implicit operator Message(string value) => new Message(value);
            public static implicit operator Message(Exception value) => new Message(value);
            public static implicit operator Message(char value) => new Message(value);
            public static implicit operator Message(byte value) => new Message(value);
            public static implicit operator Message(UInt16 value) => new Message(value);
            public static implicit operator Message(UInt32 value) => new Message(value);
            public static implicit operator Message(UInt64 value) => new Message(value);
            public static implicit operator Message(sbyte value) => new Message(value);
            public static implicit operator Message(Int16 value) => new Message(value);
            public static implicit operator Message(Int32 value) => new Message(value);
            public static implicit operator Message(Int64 value) => new Message(value);
            public static implicit operator Message(Array values) => new Message(values);
        }

        public sealed class Channel
        {
            internal readonly Action<Message, HashSet<Channel>> Handler;
            public readonly string Name;
            public readonly ConcurrentBag<Channel> Receivers;
            public Channel Log(params Message[] messages)
            {
                Logger.Log(this, messages);
                return this;
            }
            public Channel() : this(new Channel[] { }) { }
            public Channel(Action<Message> handler) : this(null, null, handler) { }
            public Channel(params Channel[] recievers) : this(null, recievers, null) { }
            public Channel(string name) : this(name, (Channel)null, null) { }
            public Channel(string name, params Channel[] recievers) : this(name, recievers, null) { }
            public Channel(string name, Action<Message> handler) : this(name, null, handler) { }
            public Channel(string name, IEnumerable<Channel> recievers, Action<Message> handler)
            {
                handler ??= (_) => { };
                this.Handler = (message, alreadyRecieved) =>
                {
                    message = String.IsNullOrEmpty(this.Name) ? message : String.Join(null, this.Name, ": ", message);
                    handler(message);
                    Array.ForEach(
                        this.Receivers
                        .Where(receiver => !alreadyRecieved.Contains(receiver))
                        .ToArray(),
                    reciever => {
                        alreadyRecieved.Add(reciever);
                        reciever.Handler(message, alreadyRecieved);
                    });
                };
                this.Name = name ?? "";
                this.Receivers = new ConcurrentBag<Channel>(
                    (recievers ?? new List<Channel>())
                    .Where(reciever => reciever != this)
                    .Distinct()
                );
            }
        }
    }
}
