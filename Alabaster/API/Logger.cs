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
        public static void Log(Channel channel, params Message[] messages) => ServerThreadManager.Run(() => channel.Handler(String.Join(' ', messages.Select(message => message.Content)), new HashSet<Channel>()));
        public static void Log(params Message[] messages) => Log(DefaultLoggers.Default, messages);
        public readonly struct Message
        {
            public readonly string Content;
            public Message(string content) => this.Content = content;
            public Message(object content) : this(
                content is System.Collections.IEnumerable ?
                FormatArray((content as IEnumerable<object>).ToArray()) :
                content.ToString()
            ) { }
            private static string FormatArray(Array array) => "[" + String.Join(", ", array) + "]";
            public Message(Array array) => this.Content = FormatArray(array);
            public static implicit operator Message(string value) => new Message(value);
            public static implicit operator Message(Exception value) => new Message(value);
            public static implicit operator Message(bool value) => new Message(value);
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
            public override string ToString() => this.Content;
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
            public Channel(MessageHandler handler) : this(null, null, handler) { }
            public Channel(params Channel[] receivers) : this(null, receivers, null) { }
            public Channel(string name) : this(name, (Channel)null, null) { }
            public Channel(string name, params Channel[] receivers) : this(name, null, receivers) { }
            public Channel(string name, MessageHandler handler) : this(name, null, handler) { }
            public Channel(string name, MessageHandler handler, params Channel[] receivers)
            {
                handler ??= (_) => { };
                this.Handler = (message, alreadyReceived) =>
                {
                    message = String.IsNullOrEmpty(this.Name) ? message : String.Join(null, this.Name, ": ", message);
                    handler(message);
                    Array.ForEach(
                        this.Receivers
                        .Where(receiver => !alreadyReceived.Contains(receiver))
                        .ToArray(),
                    receiver => {
                        alreadyReceived.Add(receiver);
                        receiver.Handler(message, alreadyReceived);
                    });
                };
                this.Name = name ?? "";
                this.Receivers = new ConcurrentBag<Channel>(
                    (receivers ?? new Channel[] { })
                    .Where(receiver => receiver != this)
                    .Distinct()
                );
            }
            public static implicit operator Channel(MessageHandler handler) => new Channel(handler);
            public static implicit operator Channel(Channel[] receivers) => new Channel(receivers);
            public static implicit operator Channel(string name) => new Channel(name);
            public static implicit operator Channel((string name, IEnumerable<Channel> receivers) args) => new Channel(args.name, args.receivers.ToArray());
            public static implicit operator Channel((string name, Channel receiver) args) => new Channel(args.name, args.receiver);
            public static implicit operator Channel((string name, MessageHandler handler) args) => new Channel(args.name, args.handler);
            public static implicit operator Channel((string name, IEnumerable<Channel> receivers, MessageHandler handler) args) => new Channel(args.name, args.receivers.ToArray(), args.handler);

            public delegate void MessageHandler(Message message);
        }
    }
}
