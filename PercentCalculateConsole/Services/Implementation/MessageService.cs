using System.Text;
using InvestCore.Domain.Helpers;
using InvestCore.Domain.Models;
using InvestCore.PercentCalculateConsole.Domain;
using PercentCalculateConsole.Domain;
using PercentCalculateConsole.Services.Interfaces;

namespace PercentCalculateConsole.Services.Implementation
{
    public class MessageService : IMessageService
    {
        private readonly IStockPortfolioService _stockPortfolioService;
        private readonly IBuyModelService _buyModelService;
        private readonly IMetricStrategy _metricStrategy;

        public MessageService(IStockPortfolioService stockPortfolioService, IBuyModelService buyModelService,
            IMetricStrategy metricStrategy)
        {
            _stockPortfolioService = stockPortfolioService;
            _buyModelService = buyModelService;
            _metricStrategy = metricStrategy;
        }

        public string GetResultMessage(StockPortfolioCalculationModel stockPortfolio)
        {
            var stockPortfolioPrices = _stockPortfolioService.GetStockProfilePrices(stockPortfolio);
            _stockPortfolioService.LoadPricesToModel(stockPortfolio, stockPortfolioPrices);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("-----------------Текущая стоимость портфеля-----------------");
            sb.AppendLine();
            sb.AppendLine(GetPricesTable(stockPortfolio, stockPortfolioPrices));
            sb.AppendLine(GetOverallMessage(stockPortfolio));

            for (int i = 0; i < stockPortfolio.MonthCountForCalculate; i++)
            {
                sb.AppendLine($"--------------------------Месяц №{i + 1}--------------------------");
                sb.AppendLine($"Инструменты для покупки: акции {stockPortfolio.Share.Ticker}, " +
                    $"гос. облигации {stockPortfolio.GosBond.Ticker}, " +
                    $"корп. облигации {stockPortfolio.CorpBond.Ticker}, " +
                    $"золото {stockPortfolio.Gold.Ticker}");
                sb.AppendLine($"Целевое распределение активов: акции {stockPortfolio.Share.TargetPercent:P2}, " +
                    $"гос. облигации {stockPortfolio.GosBond.TargetPercent:P2}, " +
                    $"корп. облигации {stockPortfolio.CorpBond.TargetPercent:P2}, " +
                    $"золото {stockPortfolio.Gold.TargetPercent:P2}");
                sb.AppendLine();
                sb.AppendLine($"Сумма для покупки: {stockPortfolio.Replenishment.SumForBuy} руб.");
                sb.AppendLine();

                var bestModel = _buyModelService.CalculateBestBuyModel(stockPortfolio.Share,
                    stockPortfolio.GosBond, stockPortfolio.CorpBond, stockPortfolio.Gold, stockPortfolio.Replenishment);
                sb.AppendLine(GetBuyMessage(bestModel));

                if (bestModel != null)
                {
                    _stockPortfolioService.UpdateOverallSum(stockPortfolio, bestModel);
                    sb.AppendLine(GetOverallMessage(stockPortfolio));
                }
            }

            return sb.ToString();
        }

        public string GetTestResultMessage(StockPortfolioCalculationModel stockPortfolio)
        {
            var stockPortfolioPrices = _stockPortfolioService.GetStockProfilePrices(stockPortfolio);
            _stockPortfolioService.LoadPricesToModel(stockPortfolio, stockPortfolioPrices);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("-----------------Текущая стоимость портфеля-----------------");
            sb.AppendLine();
            sb.AppendLine(GetPricesTable(stockPortfolio, stockPortfolioPrices));
            sb.AppendLine(GetOverallMessage(stockPortfolio));

            for (int i = 0; i < 12; i++)
            {
                sb.AppendLine($"--------------------------Месяц №{i + 1}--------------------------");
                sb.AppendLine($"Акции - Гос. облигации - Корп. облигации");
                sb.AppendLine($"Остаток средств - % откл. акции - % откл. гос. облигации - % откл. корп. облигации");
                sb.AppendLine();
                var models = new List<(BuyModel, decimal, double)>();
                for (double j = 0.005; j < 0.1; j += 0.001)
                {
                    var testModel = _buyModelService.CalculateBestBuyModel(stockPortfolio.Share,
                        stockPortfolio.GosBond, stockPortfolio.CorpBond, stockPortfolio.Gold, stockPortfolio.Replenishment, stepPercent: j);

                    if (testModel != null)
                    {
                        //var message = GetShortBuyMessage(testModel);
                        //sb.AppendLine($"{j:F3}) " + message);
                        models.Add((testModel, testModel.GetMetric(_metricStrategy), j));
                    }
                }

                var (bestModel, _, coef) = models.OrderBy(x => x.Item2).FirstOrDefault();
                sb.AppendLine($"Лучшая модель с коэффициентом = {coef:F3}:");
                sb.AppendLine(GetShortBuyMessage(bestModel));

                if (bestModel != null)
                {
                    _stockPortfolioService.UpdateOverallSum(stockPortfolio, bestModel);
                    sb.AppendLine(GetOverallMessage(stockPortfolio));
                }
            }

            return sb.ToString();
        }

        public string GetOverallMessage(StockPortfolioCalculationModel stockPortfolio)
        {
            var overall = stockPortfolio.Share.OverallSum
                + stockPortfolio.GosBond.OverallSum
                + stockPortfolio.CorpBond.OverallSum
                + stockPortfolio.Gold.OverallSum;

            return GetOverallMessage(stockPortfolio.Share.OverallSum,
                stockPortfolio.GosBond.OverallSum,
                stockPortfolio.CorpBond.OverallSum,
                stockPortfolio.Gold.OverallSum,
                overall);
        }

        public string GetOverallMessage(decimal newOverallShares, decimal newOverallGosBonds, decimal newOverallCorpBonds,
            decimal newOverallGold, decimal newOverall)
        {
            return Environment.NewLine +
                $"Акции\t\tГос. облигации\tКорп. облигации\tЗолото" + Environment.NewLine +
                $"{newOverallShares:F}\t{newOverallGosBonds:F}\t{newOverallCorpBonds:F}\t{newOverallGold:F}" + Environment.NewLine +
                $"{newOverallShares / newOverall:P4}\t{newOverallGosBonds / newOverall:P4}\t{newOverallCorpBonds / newOverall:P4}" +
                $"\t{newOverallGold / newOverall:P4}"
                + Environment.NewLine + Environment.NewLine;
        }

        public string GetBuyMessage(BuyModel? model)
        {
            if (model == null)
                return "Не удалось вычислить";

            return $"{model.ShareCounts} акций, " +
                $"{model.GosBondCounts} гос. облигаций, " +
                $"{model.CorpBondCounts} корп. облигаций, " +
                $"{model.GoldCounts} золота." + Environment.NewLine + Environment.NewLine +
                $"Отстаток средств: {model.SumDifference:F}." + Environment.NewLine +
                $"Отклонение по акциям: {model.SharePercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по гос. облигациям: {model.GosBondPercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по корп. облигациям: {model.CorpBondPercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по золоту: {model.GoldPercentDeviation:P8}";
        }

        public string? GetShortBuyMessage(BuyModel? model)
        {
            if (model == null)
                return null;

            return $"{model.ShareCounts} - {model.GosBondCounts} - {model.CorpBondCounts} - {model.GoldCounts} | {model.GetMetric(_metricStrategy):F10} | " +
                $"{model.SumDifference:F} - {model.SharePercentDeviation:P8} - {model.GosBondPercentDeviation:P8} - {model.CorpBondPercentDeviation:P8} - " +
                $"{model.GoldPercentDeviation:P8}"
                + Environment.NewLine;
        }

        private static string GetPricesTable(StockPortfolioCalculationModel stockPortfolio, Dictionary<string, decimal> stockPortfolioPrices)
        {
            var data = ToDataArrays(stockPortfolio.TickerInfos
                .Select(x => new TickerPriceInfo
                {
                    Count = x.Count,
                    Price = stockPortfolioPrices[x.Ticker],
                    Ticker = x.Ticker,
                }));

            return TableFormatHelper.GetTable(data, 0, "|", needToMakeMonospaceFont: false);
        }

        private static string[][] ToDataArrays(IEnumerable<TickerPriceInfo> models)
        {
            var data = new string[models.Count() + 2][];
            int i = 0;
            data[i++] = new[] { "№", "Ticker", "Count", "Price", "Value" };

            foreach (var model in models.OrderByDescending(x => x.Value))
            {
                data[i] = new[] {
                    $"{i++}",
                    model.Ticker,
                    model.Count.ToString(),
                    TableFormatHelper.GetFormattedNumber(model.Price),
                    TableFormatHelper.GetFormattedNumber(model.Value),
                };
            }

            data[i] = new[]
            {
                "",
                "",
                "",
                "",
                TableFormatHelper.GetFormattedNumber(models.Sum(x => x.Value)),
            };

            return data;
        }
    }
}
