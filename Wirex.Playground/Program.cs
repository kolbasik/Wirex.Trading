using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Wirex.Engine;

namespace Wirex.Playground
{
    internal class Program
    {
        private static readonly int threadCount = 10;

        private static void Main(string[] args)
        {
            ITradingEngine engine = new TradingEngine();
            var orders = new ConcurrentQueue<Order>(OrderGenerator.Generate("USD", "EUR", 0.93, 0.99));

            //Observe results
            engine.OrderClosed += OutputResult;

            //Simulate multi-threading environment
            for (var i = 0; i < threadCount; i++)
                Task.Run(() => PlaceOrder(engine, orders));


            Console.ReadLine();
        }

        private static void OutputResult(object sender, OrderArgs e)
        {
            Console.WriteLine(e.Order);
        }

        private static void PlaceOrder(ITradingEngine engine, ConcurrentQueue<Order> orders)
        {
            Order order;
            while (orders.TryDequeue(out order))
                engine.Place(order);
        }
    }
}