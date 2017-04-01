using System;
using System.Collections.Generic;
using System.Linq;

namespace Wirex.Engine
{
    public class TradingEngine : ITradingEngine
    {
        private readonly List<Order> orders = new List<Order>();

        public IEnumerable<Order> OpenOrders => orders.AsReadOnly();

        public void Place(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            OpenOrder(order);
            switch (order.Side)
            {
                case Side.Sell:
                {
                    var sell = order;
                    var buys = orders.Where(x => x.Side == Side.Buy)
                        .Where(buy => buy.Price >= sell.Price && buy.CurrencyPair.Equals(sell.CurrencyPair));
                    foreach (var buy in buys.ToList())
                    {
                        MatchOrder(buy, sell);
                    }
                    break;
                }
                case Side.Buy:
                {
                    var buy = order;
                    var sells = orders.Where(x => x.Side == Side.Sell)
                        .Where(sell => sell.Price <= buy.Price && sell.CurrencyPair.Equals(buy.CurrencyPair));
                    foreach (var sell in sells.ToList())
                    {
                        MatchOrder(sell, buy);
                    }
                    break;
                }
                default:
                    throw new NotSupportedException($"Could not able to place the order: {order}");
            }
        }

        public void MatchOrder(Order one, Order two)
        {
            if (!one.CurrencyPair.Equals(two.CurrencyPair))
                throw new InvalidOperationException("Orders should have the same currency pair.");
            if (one.Side == two.Side)
                throw new InvalidOperationException(
                    "Orders should have opposite Side e.g. Buy order can match with Sell and vice versa.");
            if (one.Side == Side.Sell && one.Price > two.Price)
                throw new InvalidOperationException(
                    "Buy order can be matched with Sell order, which have Price equal or lower than Buy order price.");
            if (one.Side == Side.Buy && one.Price < two.Price)
                throw new InvalidOperationException(
                    "Sell order can be matched with Buy order, which have Price equal or more than Sell order price.");
            if (one.RemainingAmount > two.RemainingAmount)
            {
                one.RemainingAmount -= two.RemainingAmount;
                CloseOrder(two);
            }
            else if (one.RemainingAmount < two.RemainingAmount)
            {
                two.RemainingAmount -= one.RemainingAmount;
                CloseOrder(one);
            }
            else
            {
                CloseOrder(one);
                CloseOrder(two);
            }
        }

        private void OpenOrder(Order order)
        {
            orders.Add(order);
            OrderOpened.Invoke(this, new OrderArgs(order));
        }

        private void CloseOrder(Order order)
        {
            orders.Remove(order);
            order.RemainingAmount = 0;
            OrderClosed.Invoke(this, new OrderArgs(order));
        }

        public event EventHandler<OrderArgs> OrderOpened = delegate { };

        public event EventHandler<OrderArgs> OrderClosed = delegate { };
    }
}