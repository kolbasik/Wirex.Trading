using System.Collections.Generic;
using Troschuetz.Random;
using Wirex.Engine;

namespace Wirex.Playground
{
    public class OrderGenerator
    {
        private const int Ordercount = 100;

        public static IEnumerable<Order> Generate(string baseCurrency, string quoteCurrency, double minPrice,
            double maxPrice)
        {
            var price = new TRandom();
            var amount = new TRandom();
            for (var i = 0; i < Ordercount; i++)
            {
                yield return new Order(new CurrencyPair(baseCurrency, quoteCurrency), Side.Buy,
                    decimal.Round((decimal) price.NextDouble(minPrice, maxPrice), 4), amount.Next(1, 100));
                yield return new Order(new CurrencyPair(baseCurrency, quoteCurrency), Side.Sell,
                    decimal.Round((decimal) price.NextDouble(minPrice, maxPrice), 4), amount.Next(1, 100));
            }
        }
    }
}