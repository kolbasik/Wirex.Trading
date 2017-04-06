using System;

namespace Wirex.Engine
{
    public class Order : IEquatable<Order>
    {
        public Order(CurrencyPair currencyPair, Side side, decimal price, decimal amount)
        {
            Id = Guid.NewGuid();
            Price = price;
            Side = side;
            Amount = amount;
            RemainingAmount = amount;
            CurrencyPair = currencyPair;
        }

        public Guid Id { get; }
        public Side Side { get; }
        public decimal Price { get; }
        public CurrencyPair CurrencyPair { get; }
        public decimal Amount { get; }
        public decimal RemainingAmount { get; set; }

        public bool Equals(Order other)
        {
            return other != null && Id.Equals(other.Id);
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
                $"{nameof(Id)}: {Id}, {nameof(Side)}: {Side}, {nameof(Price)}: {Price}, {nameof(CurrencyPair)}: {CurrencyPair}, {nameof(Amount)}: {Amount}, {nameof(RemainingAmount)}: {RemainingAmount}";
        }
    }
}