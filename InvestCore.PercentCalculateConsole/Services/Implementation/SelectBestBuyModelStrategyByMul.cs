using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Interfaces;

namespace InvestCore.PercentCalculateConsole.Services.Implementation
{
    public class SelectBestBuyModelStrategyByMul : ISelectBestBuyModelStrategy
    {
        private const decimal AdditionCoef = 0.0001m;

        public BuyModel? SelectBestModel(List<BuyModel> buyModels)
            => buyModels
                .OrderBy(x => x.SumDifference
                    * (AdditionCoef + x.SharePercentDeviation)
                    * (AdditionCoef + x.GosBondPercentDeviation)
                    * (AdditionCoef + x.CorpBondPercentDeviation))
                .FirstOrDefault();
    }
}
