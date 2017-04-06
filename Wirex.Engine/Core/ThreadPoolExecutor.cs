using System;
using System.Threading;

namespace Wirex.Engine.Core
{
    public sealed class ThreadPoolExecutor : Executor
    {
        /// <inheritdoc />
        public override void Invoke(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            ThreadPool.QueueUserWorkItem(_ => action.Invoke());
        }
    }
}