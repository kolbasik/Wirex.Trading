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
                var openedOrders = new List<Order>();
                var closedOrders = new List<Order>();

                tradingEngine.OrderOpened += (sender, args) =>
                {
                    openedOrders.Add(args.Order);
                    output.WriteLine($"Opened: {args.Order}");
                };
                tradingEngine.OrderClosed += (sender, args) =>
                {
                    closedOrders.Add(args.Order);
                    output.WriteLine($"Closed: {args.Order}");
                };

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
                    output.WriteLine($"Total: {openedOrders.Sum(x => x.Amount)}, Remaining: {openedOrders.Sum(x => x.RemainingAmount)}, Closed: {closedOrders.Sum(x => x.Amount)}");
                    output.WriteLine("--------------------------------");
                }

                // assert
                Assert.Equal(orderCount, openedOrders.Count);
                Assert.InRange(closedOrders.Count, 0, orderCount);
                Assert.Equal(orders, openedOrders);
                Assert.Equal(openedOrders.Distinct().OrderBy(x => x.Id), openedOrders.OrderBy(x => x.Id));
                Assert.Equal(closedOrders.Distinct().OrderBy(x => x.Id), closedOrders.OrderBy(x => x.Id));
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

                // act
                var openedOrders = new List<Order>();
                var closedOrders = new List<Order>();
                tradingEngine.OrderOpened += (sender, args) => openedOrders.Add(args.Order);
                tradingEngine.OrderClosed += (sender, args) => closedOrders.Add(args.Order);
                foreach (var order in orders)
                {
                    tradingEngine.Place(order);
                }
                await dispatcher.Submit(() => output.WriteLine("done."));

                // assert
                Assert.Equal(orderCount, openedOrders.Count);
                Assert.InRange(closedOrders.Count, 0, orderCount);
                Assert.Equal(orders, openedOrders);
                Assert.Equal(openedOrders.Distinct().OrderBy(x => x.Id), openedOrders.OrderBy(x => x.Id));
                Assert.Equal(closedOrders.Distinct().OrderBy(x => x.Id), closedOrders.OrderBy(x => x.Id));
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

                // act
                var openedOrders = new List<Order>();
                var closedOrders = new List<Order>();
                tradingEngine.OrderOpened += (sender, args) => openedOrders.Add(args.Order);
                tradingEngine.OrderClosed += (sender, args) => closedOrders.Add(args.Order);
                orders.ForEach(order => tradingEngine.Place(order));
                await dispatcher.Submit(() => output.WriteLine("done."));

                // assert
                Assert.Equal(orderCount, openedOrders.Count);
                Assert.InRange(closedOrders.Count, 0, orderCount);
                Assert.Equal(orders, openedOrders);
                Assert.Equal(openedOrders.Distinct().OrderBy(x => x.Id), openedOrders.OrderBy(x => x.Id));
                Assert.Equal(closedOrders.Distinct().OrderBy(x => x.Id), closedOrders.OrderBy(x => x.Id));
            }
        }
    }
}