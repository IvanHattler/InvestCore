using Tinkoff.InvestApi.V1;
using InstrumentType = InvestCore.Domain.Models.InstrumentType;

namespace InvestCore.TinkoffApi.Domain
{
    public class SymbolModel
    {
        public string Symbol { get; set; } = string.Empty;
        public InstrumentType Type { get; set; }
        public string Figi { get; set; } = string.Empty;

        /// <summary>
        /// Номинал. Только для облигаций
        /// </summary>
        public MoneyValue? Nominal { get; set; }
    }
}
