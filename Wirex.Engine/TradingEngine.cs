using System;

namespace Wirex.Engine
{
    public class TradingEngine: ITradingEngine
    {
        public void Place(Order order)
        {
            if (OrderClosed != null) OrderClosed(this, new OrderArgs(order));
        }

        public event EventHandler<OrderArgs> OrderOpened;

        public event EventHandler<OrderArgs> OrderClosed;
    }
}