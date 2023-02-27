using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvestCore.PercentCalculateConsole.Services.Implementation
{
    public class BuyModelService : IBuyModelService
    {
        protected const decimal MinPercentForBuy = 0.9m;
        private ISelectBestBuyModelStrategy _selectBestBuyModelStrategy;
        private ILogger _logger;

        public BuyModelService(ISelectBestBuyModelStrategy selectBestBuyModelStrategy, ILogger logger)
        {
            _selectBestBuyModelStrategy = selectBestBuyModelStrategy;
            _logger = logger;
        }

        public BuyModel? CalculateBestBuyModel(
            InstrumentCalculationModel shareDto,
            InstrumentCalculationModel gosBondDto,
            InstrumentCalculationModel corpBondDto,
            ReplenishmentModel replenishmentDto)
        {
            decimal sumForBuy = replenishmentDto.SumForBuy;
            decimal minSumForBuy = MinPercentForBuy * sumForBuy;

            var maxSharesCount = (int)(sumForBuy / shareDto.Price);
            var maxGosBondsCount = (int)(sumForBuy / gosBondDto.Price);
            var maxCorpBondsCount = (int)(sumForBuy / corpBondDto.Price);

            int sharesStep = GetStep(maxSharesCount);
            var gosBondsStep = GetStep(maxGosBondsCount);
            var corpBondsStep = GetStep(maxCorpBondsCount);

            var buyModels = new List<BuyModel>();

            //Генерация вариантов покупки
            for (int countShares = 0; countShares < maxSharesCount; countShares += sharesStep)
            {
                for (int countGosBonds = 0; countGosBonds < maxGosBondsCount; countGosBonds += gosBondsStep)
                {
                    for (int countCorpBonds = 0; countCorpBonds < maxCorpBondsCount; countCorpBonds += corpBondsStep)
                    {
                        var sharesPrice = countShares * shareDto.Price;
                        var gosBondsPrice = countGosBonds * gosBondDto.Price;
                        var corpBondsPrice = countCorpBonds * corpBondDto.Price;
                        var price = sharesPrice + gosBondsPrice + corpBondsPrice;

                        if (price < sumForBuy && price > minSumForBuy)
                        {
                            var newOverallShares = shareDto.OverallSum + sharesPrice;
                            var newOverallGosBonds = gosBondDto.OverallSum + gosBondsPrice;
                            var newOverallCorpBonds = corpBondDto.OverallSum + corpBondsPrice;
                            var newOverall = newOverallShares + newOverallGosBonds + newOverallCorpBonds;

                            buyModels.Add(GetBuyModel(shareDto,
                                gosBondDto,
                                corpBondDto,
                                replenishmentDto,
                                countShares,
                                countGosBonds,
                                countCorpBonds,
                                price,
                                newOverallShares,
                                newOverallGosBonds,
                                newOverallCorpBonds,
                                newOverall));
                        }
                    }
                }
            }

            _logger.LogInformation("Выбрано вариантов моделей: {count}", buyModels.Count);
            return _selectBestBuyModelStrategy.SelectBestModel(buyModels);
        }

        private static int GetStep(int maxSharesCount)
        {
            const double mul = .1;
            int step = (int)(maxSharesCount * mul);
            return step > 0 ? step : 1;
        }

        private static BuyModel GetBuyModel(InstrumentCalculationModel shareDto, InstrumentCalculationModel gosBondDto, InstrumentCalculationModel corpBondDto, ReplenishmentModel replenishmentDto, int countShares, int countGosBonds, int countCorpBonds, decimal price, decimal newOverallShares, decimal newOverallGosBonds, decimal newOverallCorpBonds, decimal newOverall)
        {
            return new BuyModel()
            {
                ShareCounts = countShares,
                GosBondCounts = countGosBonds,
                CorpBondCounts = countCorpBonds,
                SumDifference = replenishmentDto.ReplenishmentAmount + replenishmentDto.CurrentSum - price,
                SharePercentDeviation = Math.Abs(shareDto.TargetPercent - newOverallShares / newOverall),
                GosBondPercentDeviation = Math.Abs(gosBondDto.TargetPercent - newOverallGosBonds / newOverall),
                CorpBondPercentDeviation = Math.Abs(corpBondDto.TargetPercent - newOverallCorpBonds / newOverall),
            };
        }
    }
}
