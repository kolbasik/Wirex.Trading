using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wirex.Engine.Tests
{
    public sealed class TradingEngineTests : IDisposable
    {
        private readonly TradingEngine tradingEngine;

        public TradingEngineTests()
        {
            tradingEngine = new TradingEngine();
        }

        public void Dispose()
        {
            tradingEngine.Dispose();
        }

        [Fact]
        public async Task Example()
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var given = new List<Order>
            {
                new Order(currency, Side.Buy, 560.50m, 50),
                new Order(currency, Side.Buy, 560.55m, 50),
                new Order(currency, Side.Buy, 560.60m, 40),
                new Order(currency, Side.Sell, 580.60m, 100)
            };
            given.ForEach(order => tradingEngine.Place(order));
            var expected = new Dictionary<Order, decimal>()
            {
                {given[0], given[0].Amount},
                {given[2], 10m},
                {given[3], given[3].Amount}
            };

            // act
            await tradingEngine.Place(new Order(currency, Side.Sell, 560.53m, 80));

            // assert
            var actual = tradingEngine.OpenOrders.ToList();
            Assert.Equal(expected.Keys, actual);
            Assert.Equal(expected.Values, actual.Select(x => x.RemainingAmount));
        }

        [Theory]
        [InlineData(Side.Buy)]
        [InlineData(Side.Sell)]
        public void Trading_Engine_should_raise_OrderPlaced_event_if_once_order_is_places(Side side)
        {
            // arrange
            var expected = new Order(new CurrencyPair("USD", "EUR"), side, Decimal.One, Decimal.MaxValue);
            var autoResetEvent = new AutoResetEvent(false);

            // act
            Order actual = null;
            tradingEngine.OrderOpened += (sender, args) =>
            {
                actual = args.Order;
                autoResetEvent.Set();
            };
            tradingEngine.Place(expected);

            // assert
            autoResetEvent.WaitOne();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public void Trading_Engine_should_raise_OrderClosed_event_if_once_order_is_fullfilled(Side left, Side right)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var expected = new Order(currency, left, price: 10, amount: 25);
            var autoResetEvent = new AutoResetEvent(false);

            // act
            Order actual = null;
            tradingEngine.OrderClosed += (sender, args) =>
            {
                actual = args.Order;
                autoResetEvent.Set();
            };
            tradingEngine.Place(expected);
            tradingEngine.Place(new Order(currency, right, expected.Price, expected.Amount + 1));

            // assert
            autoResetEvent.WaitOne();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public async Task Order_can_match_only_if_orders_have_the_same_CurrencyPair(Side oneSide, Side twoSide)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, price: 10, amount: 25);
            var two = new Order(currency, twoSide, price: 10, amount: 25);

            // act
            await tradingEngine.MatchOrder(one, two);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public async Task Order_match_should_raise_an_exception_if_orders_does_not_have_the_same_CurrencyPair(Side oneSide,
            Side twoSide)
        {
            // arrange
            var one = new Order(new CurrencyPair("USD", "EUR"), oneSide, price: 10, amount: 25);
            var two = new Order(new CurrencyPair("EUR", "USD"), twoSide, price: 10, amount: 25);

            // act
            await Assert.ThrowsAsync<InvalidOperationException>(() => tradingEngine.MatchOrder(one, two));
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public async Task Order_can_match_only_if_orders_have_opposite_sides(Side oneSide, Side twoSide)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, price: 10, amount: 25);
            var two = new Order(currency, twoSide, price: 10, amount: 25);

            // act
            await tradingEngine.MatchOrder(one, two);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Buy)]
        [InlineData(Side.Sell, Side.Sell)]
        public async Task Order_match_should_raise_an_exception_if_orders_does_have_opposite_sides(Side oneSide, Side twoSide)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, price: 10, amount: 25);
            var two = new Order(currency, twoSide, price: 10, amount: 25);

            // act
            await Assert.ThrowsAsync<InvalidOperationException>(() => tradingEngine.MatchOrder(one, two));
        }

        [Theory]
        [InlineData(Side.Buy, 15, Side.Sell, 10)]
        [InlineData(Side.Buy, 12.5, Side.Sell, 12.5)]
        [InlineData(Side.Sell, 10, Side.Buy, 15)]
        [InlineData(Side.Sell, 12.5, Side.Buy, 12.5)]
        public async Task Order_can_match_only_if_Sell_order_has_a_price_equal_or_lower_than_Buy_order_price(Side oneSide,
            decimal onePrice, Side twoSide, decimal twoPrice)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, price: onePrice, amount: 25);
            var two = new Order(currency, twoSide, price: twoPrice, amount: 25);

            // act
            await tradingEngine.MatchOrder(one, two);
        }

        [Theory]
        [InlineData(Side.Buy, 10, Side.Sell, 15)]
        [InlineData(Side.Sell, 20, Side.Buy, 15)]
        public async Task Order_match_should_raise_an_exception_if_orders_does_have_opposite_sides2(Side oneSide,
            decimal onePrice, Side twoSide, decimal twoPrice)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, price: onePrice, amount: 25);
            var two = new Order(currency, twoSide, price: twoPrice, amount: 25);

            // act
            await Assert.ThrowsAsync<InvalidOperationException>(() => tradingEngine.MatchOrder(one, two));
        }
    }
}