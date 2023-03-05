namespace PercentCalculateConsole.Domain
{
    public class PriceInfo
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal Value => Count * Price;
    }
}
