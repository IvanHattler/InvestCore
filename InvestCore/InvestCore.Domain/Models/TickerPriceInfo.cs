using System.Text.Json.Serialization;

namespace InvestCore.Domain.Models
{
    public class TickerPriceInfo : TickerInfoWithCount
    {
        public decimal Price { get; set; }

        [JsonIgnore]
        public decimal Value => Count * Price;
    }
}
