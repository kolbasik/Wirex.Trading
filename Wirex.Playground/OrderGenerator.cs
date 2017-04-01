using System.Collections.Generic;
using Troschuetz.Random;
using Wirex.Engine;

namespace Wirex.Playground
{
    public sealed class OrderGenerator
    {
        private readonly int orderCount;

        public OrderGenerator(int orderCount)
        {
            this.orderCount = orderCount;
        }

        public IEnumerable<Order> Generate(
            string baseCurrency,
            string quoteCurrency,
            double minPrice,
            double maxPrice)
        {
            var random = new TRandom();
            for (var i = 0; i < orderCount; i++)
            {
                yield return new Order(new CurrencyPair(baseCurrency, quoteCurrency), Side.Buy,
                    decimal.Round((decimal) random.NextDouble(minPrice, maxPrice), 4), random.Next(1, 100));
                yield return new Order(new CurrencyPair(baseCurrency, quoteCurrency), Side.Sell,
                    decimal.Round((decimal) random.NextDouble(minPrice, maxPrice), 4), random.Next(1, 100));
            }
        }
    }
}