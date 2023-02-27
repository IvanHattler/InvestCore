﻿using System.Text.Json.Serialization;

namespace InvestCore.Domain.Models
{
    public class TickerInfoModel
    {
        public string DisplayName { get; set; }

        public string Ticker { get; set; }

        public InstrumentType Type { get; set; }

        public int Count { get; set; }

        public decimal LastPrice { get; set; }

        public decimal Price { get; set; }

        [JsonIgnore]
        public decimal Value => Count * Price;

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
            Type = type;
        }
    }
}
