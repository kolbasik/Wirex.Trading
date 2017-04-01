namespace Wirex.Engine
{
    public class CurrencyPair
    {
        public CurrencyPair(string baseCurrency, string quoteCurrency)
        {
            BaseCurrency = baseCurrency;
            QuoteCurrency = quoteCurrency;
        }

        public string BaseCurrency { get; }
        public string QuoteCurrency { get; }
        public override string ToString()
        {
            return $"{BaseCurrency}/,{QuoteCurrency}";
        }

        protected bool Equals(CurrencyPair other)
        {
            return string.Equals(BaseCurrency, other.BaseCurrency) && string.Equals(QuoteCurrency, other.QuoteCurrency);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CurrencyPair) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BaseCurrency?.GetHashCode() ?? 0)*397) ^ (QuoteCurrency?.GetHashCode() ?? 0);
            }
        }

       
    }
}