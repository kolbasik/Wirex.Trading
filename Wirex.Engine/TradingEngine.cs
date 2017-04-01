using System;

namespace Wirex.Engine
{
    public class TradingEngine : ITradingEngine
    {
        public void Place(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            OrderClosed.Invoke(this, new OrderArgs(order));
        }

        public event EventHandler<OrderArgs> OrderOpened = delegate { };

        public event EventHandler<OrderArgs> OrderClosed = delegate { };
    }
}