using System;
using System.Threading.Tasks;

namespace Wirex.Engine
{
    public interface IWorker : IDisposable
    {
        Task Invoke(Action action);
    }
}