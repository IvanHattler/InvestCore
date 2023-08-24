using System.Text.Json.Serialization;

namespace InvestCore.Domain.Models
{
    public class TickerInfoModel : TickerPriceInfo
    {
        public string DisplayName { get; set; }

        public decimal LastPrice { get; set; }

        [JsonIgnore]
        public decimal Difference => Value - LastValue;

        [JsonIgnore]
        public decimal DifferencePercent => LastPrice == 0 ? 1 : (Price - LastPrice) / LastPrice;

        [JsonIgnore]
        public decimal LastValue => Count * LastPrice;

        public TickerInfoModel(string ticker, string displayName, int count, InstrumentType type)
        {
            Ticker = ticker ?? throw new ArgumentNullException(nameof(ticker));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Count = count;
            TickerType = type;
        }

        public TickerInfoModel()
        {
        }
    }
}
