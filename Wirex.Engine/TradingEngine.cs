using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Wirex.Engine
{
    public class TradingEngine : ITradingEngine, IDisposable
    {
        private readonly List<Order> orders;
        private readonly Thread thread;
        private Dispatcher dispatcher;

        public IEnumerable<Order> OpenOrders => orders.AsReadOnly();

        public TradingEngine()
        {
            orders = new List<Order>();
            var autoResetEvent = new AutoResetEvent(false);
            thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                autoResetEvent.Set();
                Dispatcher.Run();
            }) {IsBackground = true};
            thread.Start();
            autoResetEvent.WaitOne();
        }

        public void Dispose()
        {
            this.thread.Abort();
        }

        public Task Place(Order order)
        {
            return Invoke(() => PlaceSafe(order));
        }

        public Task MatchOrder(Order one, Order two)
        {
            return Invoke(() => MatchOrderSafe(one, two));
        }

        public Task Invoke(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.BeginInvoke(new Action(() =>
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
            }));
            return tcs.Task;
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

        private void MatchOrderSafe(Order one, Order two)
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