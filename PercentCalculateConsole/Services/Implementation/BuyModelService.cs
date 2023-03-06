using Microsoft.Extensions.Logging;
using PercentCalculateConsole.Domain;
using PercentCalculateConsole.Services.Interfaces;

namespace PercentCalculateConsole.Services.Implementation
{
    public class BuyModelService : IBuyModelService
    {
        protected const decimal MinPercentForBuy = 0.9m;
        private readonly IMetricStrategy _metricStrategy;
        private readonly ILogger _logger;

        public BuyModelService(IMetricStrategy metricStrategy, ILogger logger)
        {
            _metricStrategy = metricStrategy;
            _logger = logger;
        }

        public BuyModel? CalculateBestBuyModel(
            InstrumentCalculationModel shareDto,
            InstrumentCalculationModel gosBondDto,
            InstrumentCalculationModel corpBondDto,
            InstrumentCalculationModel gold,
            ReplenishmentModel replenishmentDto,
            double stepPercent = .1)
        {
            decimal sumForBuy = replenishmentDto.SumForBuy;
            decimal minSumForBuy = MinPercentForBuy * sumForBuy;

            var maxSharesCount = (int)(sumForBuy / shareDto.Price);
            var maxGosBondsCount = (int)(sumForBuy / gosBondDto.Price);
            var maxCorpBondsCount = (int)(sumForBuy / corpBondDto.Price);
            var maxGoldCount = (int)(sumForBuy / gold.Price);

            int sharesStep = GetStep(maxSharesCount, stepPercent);
            var gosBondsStep = GetStep(maxGosBondsCount, stepPercent);
            var corpBondsStep = GetStep(maxCorpBondsCount, stepPercent);
            var goldStep = GetStep(maxGoldCount, stepPercent);

            var buyModels = new List<BuyModel>();

            //Генерация вариантов покупки
            for (int countShares = 0; countShares < maxSharesCount; countShares += sharesStep)
            {
                for (int countGosBonds = 0; countGosBonds < maxGosBondsCount; countGosBonds += gosBondsStep)
                {
                    for (int countCorpBonds = 0; countCorpBonds < maxCorpBondsCount; countCorpBonds += corpBondsStep)
                    {
                        for (int countGold = 0; countGold < maxGoldCount; countGold++)
                        {
                            var sharesPrice = countShares * shareDto.Price;
                            var gosBondsPrice = countGosBonds * gosBondDto.Price;
                            var corpBondsPrice = countCorpBonds * corpBondDto.Price;
                            var goldPrice = countGold * gold.Price;
                            var price = sharesPrice + gosBondsPrice + corpBondsPrice + goldPrice;

                            if (price < sumForBuy && price > minSumForBuy)
                            {
                                var newOverallShares = shareDto.OverallSum + sharesPrice;
                                var newOverallGosBonds = gosBondDto.OverallSum + gosBondsPrice;
                                var newOverallCorpBonds = corpBondDto.OverallSum + corpBondsPrice;
                                var newOverallGold = gold.OverallSum + goldPrice;
                                var newOverall = newOverallShares + newOverallGosBonds + newOverallCorpBonds + newOverallGold;

                                buyModels.Add(new BuyModel()
                                {
                                    ShareCounts = countShares,
                                    GosBondCounts = countGosBonds,
                                    CorpBondCounts = countCorpBonds,
                                    GoldCounts = countGold,
                                    SumDifference = replenishmentDto.ReplenishmentAmount + replenishmentDto.CurrentSum - price,
                                    SharePercentDeviation = Math.Abs(shareDto.TargetPercent - newOverallShares / newOverall),
                                    GosBondPercentDeviation = Math.Abs(gosBondDto.TargetPercent - newOverallGosBonds / newOverall),
                                    CorpBondPercentDeviation = Math.Abs(corpBondDto.TargetPercent - newOverallCorpBonds / newOverall),
                                    GoldPercentDeviation = Math.Abs(gold.TargetPercent - newOverallGold / newOverall),
                                });
                            }
                        }
                    }
                }
            }

            return buyModels
                .OrderBy(x => x.GetMetric(_metricStrategy))
                .FirstOrDefault();
        }

        private static int GetStep(int maxSharesCount, double coef)
        {
            int step = (int)(maxSharesCount * coef);
            return step > 0 ? step : 1;
        }
    }
}
