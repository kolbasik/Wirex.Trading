using System;

namespace Wirex.Engine
{
    public class Order
    {
        public Order(CurrencyPair currencyPair, Side side, decimal price, decimal amount)
        {
            Id = Guid.NewGuid();
            CurrencyPair = currencyPair;
            Price = price;
            Side = side;
            Amount = amount;
            RemainingAmount = amount;
        }

        public Guid Id { get; }
        public CurrencyPair CurrencyPair { get; }
        public decimal Price { get; }
        public Side Side { get; }
        public decimal Amount { get; }
        public decimal RemainingAmount { get; set; }

        protected bool Equals(Order other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Order) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return
                $"Id: {Id}, CurrencyPair: {CurrencyPair}, Price: {Price}, Side: {Side}, Amount: {Amount}, RemainingAmount: {RemainingAmount}";
        }
    }
}