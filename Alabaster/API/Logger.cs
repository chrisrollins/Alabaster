﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Alabaster
{
    using MessageHandler_A = Func<Logger.Message, Logger.Message>;
    using MessageHandler_B = Action<Logger.Message>;
    public static partial class Logger
    {
        public static void Log(Channel channel, params Message[] messages) => InternalQueueManager.SetupQueue.Run(() => channel.Handler(String.Join(' ', messages.Select(message => message.Content)), new HashSet<Channel>()));
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
            public Channel(MessageHandler handler) : this(null, MessageHandler.None, handler) { }
            public Channel(params Channel[] receivers) : this(null, receivers, null) { }
            public Channel(string name) : this(name, (Channel)null, null) { }
            public Channel(string name, params Channel[] receivers) : this(name, MessageHandler.None, receivers) { }
            public Channel(string name, MessageHandler handler) : this(name, MessageHandler.None, handler) { }
            public Channel(string name, MessageHandler handler, params Channel[] receivers)
            {
                this.Handler = (message, alreadyReceived) =>
                {
                    message = String.IsNullOrEmpty(this.Name) ? message : String.Join(null, this.Name, ": ", message);
                    handler.handler(message);
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
            public static implicit operator Channel((string name, MessageHandler_A handler) args) => new Channel(args.name, args.handler);
            public static implicit operator Channel((string name, MessageHandler_B handler) args) => new Channel(args.name, args.handler);
            public static implicit operator Channel((string name, MessageHandler handler, IEnumerable<Channel> receivers) args) => new Channel(args.name, args.handler, args.receivers.ToArray());
            public static implicit operator Channel((string name, MessageHandler_A handler, IEnumerable<Channel> receivers) args) => new Channel(args.name, args.handler, args.receivers.ToArray());
            public static implicit operator Channel((string name, MessageHandler_B handler, IEnumerable<Channel> receivers) args) => new Channel(args.name, args.handler, args.receivers.ToArray());
            public static implicit operator Channel((string name, MessageHandler handler, Channel receiver) args) => new Channel(args.name, args.handler, args.receiver);
            public static implicit operator Channel((string name, MessageHandler_A handler, Channel receiver) args) => new Channel(args.name, args.handler, args.receiver);
            public static implicit operator Channel((string name, MessageHandler_B handler, Channel receiver) args) => new Channel(args.name, args.handler, args.receiver);

            public struct MessageHandler
            {
                internal MessageHandler_A handler;
                public MessageHandler(MessageHandler_A handler) => this.handler = handler;
                public MessageHandler(MessageHandler_B handler) => this.handler = m =>
                {
                    handler(m);
                    return m;
                };

                public static implicit operator MessageHandler(MessageHandler_A handler) => new MessageHandler(handler);
                public static implicit operator MessageHandler(MessageHandler_B handler) => new MessageHandler(handler);
                internal static MessageHandler None = new MessageHandler(_ => { });
            }
        }
    }
}
