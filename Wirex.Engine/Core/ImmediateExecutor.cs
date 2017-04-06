using System;

namespace Wirex.Engine.Core
{
    public sealed class ImmediateExecutor : Executor
    {
        /// <inheritdoc />
        public override void Invoke(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action.Invoke();
        }
    }
}