using Tinkoff.InvestApi.V1;
using InstrumentType = InvestCore.Domain.Models.InstrumentType;

namespace InvestCore.TinkoffApi.Domain
{
    public class SymbolModel
    {
        public string Symbol { get; set; }
        public InstrumentType Type { get; set; }
        public string Figi { get; set; }

        /// <summary>
        /// Номинал. Только для облигаций
        /// </summary>
        public MoneyValue? Nominal { get; set; }
    }
}
