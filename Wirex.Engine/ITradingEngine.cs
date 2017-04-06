using System;
using System.Threading.Tasks;

namespace Wirex.Engine
{
    public interface ITradingEngine
    {
        void Place(Order order);
        event EventHandler<OrderArgs> OrderOpened;
        event EventHandler<OrderArgs> OrderClosed;
    }
}