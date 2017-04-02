using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Wirex.Engine
{
    public sealed class Worker : IWorker, IDisposable
    {
        private readonly Dispatcher dispatcher;

        public Worker() : this(CreateDispatcher())
        {
        }

        public Worker(Dispatcher dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            this.dispatcher = dispatcher;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.dispatcher.Thread.Abort();
        }

        public Task Invoke(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }));
            return tcs.Task;
        }

        public static Dispatcher CreateDispatcher()
        {
            var autoResetEvent = new AutoResetEvent(false);
            Dispatcher dispatcher = null;
            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                autoResetEvent.Set();
                Dispatcher.Run();
            })
            { IsBackground = true };
            thread.Start();
            autoResetEvent.WaitOne();
            return dispatcher;
        }
    }
}