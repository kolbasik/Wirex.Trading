using System;
using System.Collections.Generic;
using System.Linq;
using Wirex.Engine.Core;

namespace Wirex.Engine
{
    public class TradingEngine : ITradingEngine, IDisposable
    {
        private readonly List<Order> orders;
        private readonly Executor executor;
        private readonly Executor notifier;

        public event EventHandler<OrderArgs> OrderOpened = delegate { };
        public event EventHandler<OrderArgs> OrderClosed = delegate { };

        public IEnumerable<Order> OpenOrders => orders.AsReadOnly();
        public Executor Executor => executor;
        public Executor Notifier => notifier;

        public TradingEngine() : this(Executors.Dispatcher, Executors.ThreadPool)
        {
        }

        public TradingEngine(Func<Executor> createExecutor, Func<Executor> createNotifier)
        {
            if (createExecutor == null) throw new ArgumentNullException(nameof(createExecutor));
            if (createNotifier == null) throw new ArgumentNullException(nameof(createNotifier));
            this.executor = createExecutor();
            this.notifier = createNotifier();
            orders = new List<Order>();
        }

        public void Dispose()
        {
            executor.Dispose();
            notifier.Dispose();
        }

        public void Place(Order order)
        {
            executor.Invoke(() => PlaceSafe(order));
        }

        public void MatchOrder(Order one, Order two)
        {
            executor.Invoke(() => MatchOrderSafe(one, two));
        }

        private void PlaceSafe(Order order)
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
                        if (MatchOrderSafe(sell, buy))
                            break;
                    break;
                }
                case Side.Buy:
                {
                    var buy = order;
                    var sells = orders.Where(x => x.Side == Side.Sell)
                        .Where(sell => sell.Price <= buy.Price && sell.CurrencyPair.Equals(buy.CurrencyPair));
                    foreach (var sell in sells.ToList())
                        if (MatchOrderSafe(buy, sell))
                            break;
                    break;
                }
                default:
                    throw new NotSupportedException($"Could not able to place the order: {order}");
            }
        }

        private bool MatchOrderSafe(Order one, Order two)
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
                return false;
            }
            if (one.RemainingAmount < two.RemainingAmount)
            {
                two.RemainingAmount -= one.RemainingAmount;
                CloseOrder(one);
                return true;
            }
            CloseOrder(one);
            CloseOrder(two);
            return true;
        }

        private void OpenOrder(Order order)
        {
            orders.Add(order);
            notifier.Invoke(() => OrderOpened.Invoke(this, new OrderArgs(order)));
        }

        private void CloseOrder(Order order)
        {
            order.RemainingAmount = 0;
            orders.Remove(order);
            notifier.Invoke(() => OrderClosed.Invoke(this, new OrderArgs(order)));
        }
    }
}