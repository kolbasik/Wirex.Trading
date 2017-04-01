using System;

namespace Wirex.Engine
{
    public class CurrencyPair : IEquatable<CurrencyPair>
    {
        public CurrencyPair(string baseCurrency, string quoteCurrency)
        {
            BaseCurrency = baseCurrency;
            QuoteCurrency = quoteCurrency;
        }

        public string BaseCurrency { get; }
        public string QuoteCurrency { get; }

        public bool Equals(CurrencyPair other)
        {
            return other != null && string.Equals(BaseCurrency, other.BaseCurrency) &&
                   string.Equals(QuoteCurrency, other.QuoteCurrency);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CurrencyPair) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BaseCurrency?.GetHashCode() ?? 0) * 397) ^ (QuoteCurrency?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            return $"{BaseCurrency}/,{QuoteCurrency}";
        }
    }
}