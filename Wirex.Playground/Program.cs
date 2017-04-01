using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Wirex.Engine;

namespace Wirex.Playground
{
    internal class Program
    {
        private const int ThreadCount = 10;
        private const int OrderCount = 100;

        private static void Main(string[] args)
        {
            ITradingEngine engine = new TradingEngine();
            //Observe results
            engine.OrderClosed += OutputResult;

            //Simulate multi-threading environment
            var orders = new ConcurrentQueue<Order>(new OrderGenerator(OrderCount).Generate("USD", "EUR", 0.93, 0.99));
            for (var i = 0; i < ThreadCount; i++)
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