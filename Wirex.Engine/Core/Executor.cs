using System;
using System.Threading.Tasks;

namespace Wirex.Engine.Core
{
    public abstract class Executor : Disposable
    {
        public abstract void Invoke(Action action);

        public virtual Task Submit(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            Invoke(() =>
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
            });
            return tcs.Task;
        }
    }
}