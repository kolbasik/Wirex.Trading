using System;
using System.Threading;
using System.Windows.Threading;

namespace Wirex.Engine.Core
{
    public sealed class DispatcherExecutor : Executor
    {
        private readonly Dispatcher dispatcher;

        public DispatcherExecutor() : this(CreateDispatcher())
        {
        }

        public DispatcherExecutor(Dispatcher dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            this.dispatcher = dispatcher;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Invoke(Dispatcher.ExitAllFrames);
                dispatcher.Thread.Join();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void Invoke(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            dispatcher.BeginInvoke(action);
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
                {IsBackground = true};
            thread.Start();
            autoResetEvent.WaitOne();
            return dispatcher;
        }
    }
}