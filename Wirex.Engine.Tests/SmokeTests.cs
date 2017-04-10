using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wirex.Engine.Core;
using Xunit;
using Xunit.Abstractions;

namespace Wirex.Engine.Tests
{
    public abstract class SmokeTests : IDisposable
    {
        private readonly Executor dispatcher;
        private readonly TradingEngine tradingEngine;

        private SmokeTests(Executor dispatcher)
        {
            this.dispatcher = dispatcher;
            tradingEngine = new TradingEngine(() => dispatcher, Executors.Immediate);
        }

        public void Dispose()
        {
            dispatcher.Dispose();
        }

        private static List<Order> GenerateOrders(int orderCount)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var currency = new CurrencyPair("USD", "EUR");
            var orders = Enumerable.Range(0, orderCount).Select(i =>
                new Order(currency,
                    (i & 1) == 0 ? Side.Buy : Side.Sell,
                    decimal.Round((decimal) (0.93 + random.NextDouble() * (0.99 - 0.93)), 4),
                    random.Next(1, 100))).ToList();
            return orders;
        }

        public class Immediate : SmokeTests
        {
            private readonly ITestOutputHelper output;

            public Immediate(ITestOutputHelper output) : base(Executors.Immediate())
            {
                if (output == null) throw new ArgumentNullException(nameof(output));
                this.output = output;
            }

            [Theory]
            [InlineData(10)]
            [InlineData(25)]
            [InlineData(50)]
            public void Simulate(int orderCount)
            {
                // arrange
                var orders = GenerateOrders(orderCount);

                var spy = new TradingEngineSpy();
                spy.ListenTo(tradingEngine);

                tradingEngine.OrderOpened += (sender, args) => output.WriteLine($"Opened: {args.Order}");
                tradingEngine.OrderClosed += (sender, args) => output.WriteLine($"Closed: {args.Order}");

                // act
                foreach (var order in orders)
                {
                    output.WriteLine($"Place: {order}");
                    tradingEngine.Place(order);
                    output.WriteLine("Orders:");
                    foreach (var openOrder in tradingEngine.OpenOrders)
                    {
                        output.WriteLine(openOrder.ToString());
                    }
                    output.WriteLine("--------------------------------");
                    output.WriteLine($"Total: {spy.OpenedOrders.Sum(x => x.Amount)}, Remaining: {spy.OpenedOrders.Sum(x => x.RemainingAmount)}, Closed: {spy.ClosedOrders.Sum(x => x.Amount)}");
                    output.WriteLine("--------------------------------");
                }

                // assert
                spy.Verify(orders);
            }
        }

        public class Sequential : SmokeTests
        {
            private readonly ITestOutputHelper output;

            public Sequential(ITestOutputHelper output) : base(Executors.Dispatcher())
            {
                if (output == null) throw new ArgumentNullException(nameof(output));
                this.output = output;
            }

            [Theory]
            [InlineData(10)]
            [InlineData(100)]
            [InlineData(1000)]
            public async Task Simulate(int orderCount)
            {
                // arrange
                var orders = GenerateOrders(orderCount);

                var spy = new TradingEngineSpy();
                spy.ListenTo(tradingEngine);

                // act
                foreach (var order in orders)
                {
                    tradingEngine.Place(order);
                }
                await dispatcher.Submit(() => output.WriteLine("done."));

                // assert
                spy.Verify(orders);
            }
        }

        public class Parallel : SmokeTests
        {
            private readonly ITestOutputHelper output;

            public Parallel(ITestOutputHelper output) : base(Executors.Dispatcher())
            {
                if (output == null) throw new ArgumentNullException(nameof(output));
                this.output = output;
            }

            [Theory]
            [InlineData(10)]
            [InlineData(100)]
            [InlineData(1000)]
            public async Task Simulate(int orderCount)
            {
                // arrange
                var orders = GenerateOrders(orderCount);

                var spy = new TradingEngineSpy();
                spy.ListenTo(tradingEngine);

                // act
                orders.ForEach(order => tradingEngine.Place(order));
                await dispatcher.Submit(() => output.WriteLine("done."));

                // assert
                spy.Verify(orders);
            }
        }

        private sealed class TradingEngineSpy
        {
            public readonly List<Order> OpenedOrders;
            public readonly List<Order> ClosedOrders;

            public TradingEngineSpy()
            {
                OpenedOrders = new List<Order>();
                ClosedOrders = new List<Order>();
            }

            public void ListenTo(ITradingEngine tradingEngine)
            {
                tradingEngine.OrderOpened += (sender, args) => OpenedOrders.Add(args.Order);
                tradingEngine.OrderClosed += (sender, args) => ClosedOrders.Add(args.Order);
            }

            public void Verify(List<Order> orders)
            {
                Assert.Equal(orders, OpenedOrders);
                Assert.InRange(ClosedOrders.Count, 0, orders.Count);
                Assert.Equal(OpenedOrders.Distinct().OrderBy(x => x.Id), OpenedOrders.OrderBy(x => x.Id));
                Assert.Equal(ClosedOrders.Distinct().OrderBy(x => x.Id), ClosedOrders.OrderBy(x => x.Id));
                Assert.InRange(OpenedOrders.Sum(x => x.RemainingAmount) + ClosedOrders.Sum(x => x.Amount), 0, orders.Sum(x => x.Amount));
            }
        }
    }
}