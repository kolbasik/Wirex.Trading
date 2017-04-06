using System;

namespace Wirex.Engine.Core
{
    public abstract class Disposable : IDisposable
    {
        ~Disposable()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}