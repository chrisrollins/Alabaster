using System;

namespace Alabaster
{
    internal static class InternalExceptionHandler
    {
        private static readonly InternalQueueManager.ActionQueue IncomingExceptions = new InternalQueueManager.ActionQueue(new InternalQueueManager.ActionQueue.Configuration {
            Priority = System.Threading.ThreadPriority.BelowNormal,
            ExceptionHandler = (exception) => DefaultLoggers.Error.Log(InternalExceptionMessage(InternalExceptionCode.ErrorInsideInternalExceptionHandler)),
        });

        internal static void Rethrow(InternalExceptionCode errorCode, Action action) => Rethrow(errorCode, () =>
        {
            action();
            return 0;
        });

        internal static T Rethrow<T>(InternalExceptionCode errorCode, Func<T> action)
        {
            try { return action(); }
            catch (Exception exception) { throw new InternalException(errorCode, exception); }
        }

        internal static T Try<T>(InternalExceptionCode rethrowCode, Func<T> action) => Try(() => Rethrow(rethrowCode, action));

        internal static void Try(InternalExceptionCode rethrowCode, Action action) => Try(() => Rethrow(rethrowCode, action));

        internal static void Try(Action action) => Try(() => {
            action();
            return 0;
        });

        internal static T Try<T>(Func<T> action)
        {
            try { return action(); }
            catch (Exception exception)
            {
                Handle(exception);
                return default;
            }
        }

        internal static void Handle(Exception exception)
        {
            IncomingExceptions.Run(() =>
            {
                switch (exception)
                {
                    case InternalException internalException:
                        internalException.Resolve();
                        break;
                    default:
                        DefaultLoggers.Error
                        .Log("Unhandled exception:")
                        .Log(exception.Message);
                        break;
                }
            });
        }

        internal static string InternalExceptionMessage(InternalExceptionCode errorCode) => $"Unexpected internal exception. Error code: {(int)errorCode}";
    }

    internal enum InternalExceptionCode
    {
        Unspecified,
        ErrorInsideInternalExceptionHandler,
        ActionQueueTryTake,
        ActionQueueRejectedOperation,
        FailedToRegisterIntervalThreadID,
    }

    internal sealed class InternalException : Exception 
    {
        internal void Resolve()
        {            
            DefaultLoggers.Error.Log(this.Message);
        }
        internal InternalException(InternalExceptionCode errorCode) : base(InternalExceptionHandler.InternalExceptionMessage(errorCode), new Exception()) { }
        internal InternalException(InternalExceptionCode errorCode, Exception innerException) : base(InternalExceptionHandler.InternalExceptionMessage(errorCode), innerException) { }
    }
}