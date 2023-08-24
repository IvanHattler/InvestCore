namespace InvestCore.Domain.Models
{
    public class TickerPriceInfo : TickerInfoWithCount
    {
        public decimal Price { get; set; }

        public decimal Value => Count * Price;
    }
}
