using System;

namespace Wirex.Engine
{
    public class OrderArgs : EventArgs
    {
        public OrderArgs(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            Order = order;
        }

        public Order Order { get; }
    }
}