using System;
using System.Collections.Generic;
using System.Linq;
using Wirex.Engine.Core;
using Xunit;

namespace Wirex.Engine.Tests
{
    public sealed class TradingEngineTests
    {
        private readonly TradingEngine tradingEngine;

        public TradingEngineTests()
        {
            tradingEngine = new TradingEngine(Executors.Immediate, Executors.Immediate);
        }

        [Theory]
        [InlineData(Side.Buy)]
        [InlineData(Side.Sell)]
        public void Trading_Engine_should_raise_OrderPlaced_event_if_once_order_is_places(Side side)
        {
            // arrange
            var expected = new Order(new CurrencyPair("USD", "EUR"), side, decimal.One, decimal.MaxValue);

            // act
            Order actual = null;
            tradingEngine.OrderOpened += (sender, args) =>
            {
                actual = args.Order;
            };
            tradingEngine.Place(expected);

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public void Trading_Engine_should_raise_OrderClosed_event_if_once_order_is_fullfilled(Side left, Side right)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var expected = new Order(currency, left, 10, 25);

            // act
            Order actual = null;
            tradingEngine.OrderClosed += (sender, args) => actual = args.Order;
            tradingEngine.Place(expected);
            tradingEngine.Place(new Order(currency, right, expected.Price, expected.Amount + 1));

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public void Order_can_match_only_if_orders_have_the_same_CurrencyPair(Side oneSide, Side twoSide)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, 10, 25);
            var two = new Order(currency, twoSide, 10, 25);

            // act
            tradingEngine.MatchOrder(one, two);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public void Order_match_should_raise_an_exception_if_orders_does_not_have_the_same_CurrencyPair(Side oneSide, Side twoSide)
        {
            // arrange
            var one = new Order(new CurrencyPair("USD", "EUR"), oneSide, 10, 25);
            var two = new Order(new CurrencyPair("EUR", "USD"), twoSide, 10, 25);

            // act
            Assert.Throws<InvalidOperationException>(() => tradingEngine.MatchOrder(one, two));
        }

        [Theory]
        [InlineData(Side.Buy, Side.Sell)]
        [InlineData(Side.Sell, Side.Buy)]
        public void Order_can_match_only_if_orders_have_opposite_sides(Side oneSide, Side twoSide)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, 10, 25);
            var two = new Order(currency, twoSide, 10, 25);

            // act
            tradingEngine.MatchOrder(one, two);
        }

        [Theory]
        [InlineData(Side.Buy, Side.Buy)]
        [InlineData(Side.Sell, Side.Sell)]
        public void Order_match_should_raise_an_exception_if_orders_does_have_opposite_sides(Side oneSide, Side twoSide)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, 10, 25);
            var two = new Order(currency, twoSide, 10, 25);

            // act
            Assert.Throws<InvalidOperationException>(() => tradingEngine.MatchOrder(one, two));
        }

        [Theory]
        [InlineData(Side.Buy, 15, Side.Sell, 10)]
        [InlineData(Side.Buy, 12.5, Side.Sell, 12.5)]
        [InlineData(Side.Sell, 10, Side.Buy, 15)]
        [InlineData(Side.Sell, 12.5, Side.Buy, 12.5)]
        public void Order_can_match_only_if_Sell_order_has_a_price_equal_or_lower_than_Buy_order_price(
            Side oneSide, decimal onePrice, Side twoSide, decimal twoPrice)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, onePrice, 25);
            var two = new Order(currency, twoSide, twoPrice, 25);

            // act
            tradingEngine.MatchOrder(one, two);
        }

        [Theory]
        [InlineData(Side.Buy, 10, Side.Sell, 15)]
        [InlineData(Side.Sell, 20, Side.Buy, 15)]
        public void Order_match_should_raise_an_exception_if_orders_does_have_opposite_sides(
            Side oneSide, decimal onePrice, Side twoSide, decimal twoPrice)
        {
            // arrange
            var currency = new CurrencyPair("USD", "EUR");
            var one = new Order(currency, oneSide, onePrice, 25);
            var two = new Order(currency, twoSide, twoPrice, 25);

            // act
            Assert.Throws<InvalidOperationException>(() => tradingEngine.MatchOrder(one, two));
        }

        [Fact]
        public void Example()
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
            var expected = new Dictionary<Order, decimal>
            {
                {given[0], given[0].Amount},
                {given[2], 10m},
                {given[3], given[3].Amount}
            };

            // act
            tradingEngine.Place(new Order(currency, Side.Sell, 560.53m, 80));

            // assert
            var actual = tradingEngine.OpenOrders.ToList();
            Assert.Equal(expected.Keys, actual);
            Assert.Equal(expected.Values, actual.Select(x => x.RemainingAmount));
        }
    }
}