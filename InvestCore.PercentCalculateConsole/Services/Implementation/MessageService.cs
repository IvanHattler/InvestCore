using System.Text;
using InvestCore.Domain.Helpers;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Interfaces;

namespace InvestCore.PercentCalculateConsole.Services.Implementation
{
    public class MessageService : IMessageService
    {
        private readonly IStockPortfolioService _stockPortfolioService;
        private readonly IBuyModelService _buyModelService;
        private readonly IShareService _shareService;

        public MessageService(IStockPortfolioService stockPortfolioService, IBuyModelService buyModelService, IShareService shareService)
        {
            _stockPortfolioService = stockPortfolioService;
            _buyModelService = buyModelService;
            _shareService = shareService;
        }

        public string GetResultMessage(StockPortfolioCalculationModel stockPortfolio)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("-----------------Текущая стоимость портфеля-----------------");
            sb.AppendLine();
            sb.AppendLine(GetPricesTable(stockPortfolio));
            sb.AppendLine(GetOverallMessage(stockPortfolio));

            for (int i = 0; i < 12; i++)
            {
                var bestModel = _buyModelService.CalculateBestBuyModel(stockPortfolio.Share,
                    stockPortfolio.GosBond, stockPortfolio.CorpBond, stockPortfolio.Replenishment);

                sb.AppendLine($"--------------------------Месяц №{i + 1}--------------------------");
                sb.AppendLine($"Инструменты для покупки: акции {stockPortfolio.Share.Ticker}, " +
                    $"гос. облигации {stockPortfolio.GosBond.Ticker}, " +
                    $"корп. облигации {stockPortfolio.CorpBond.Ticker}");
                sb.AppendLine();
                sb.AppendLine($"Сумма для покупки: {stockPortfolio.Replenishment.SumForBuy} руб.");
                sb.AppendLine();
                sb.AppendLine(GetBuyMessage(bestModel));

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
                + stockPortfolio.CorpBond.OverallSum;

            return GetOverallMessage(stockPortfolio.Share.OverallSum,
                stockPortfolio.GosBond.OverallSum,
                stockPortfolio.CorpBond.OverallSum, overall);
        }

        public string GetOverallMessage(decimal newOverallShares, decimal newOverallGosBonds, decimal newOverallCorpBonds, decimal newOverall)
        {
            return Environment.NewLine +
                $"Акции\t\tГос. облигации\tКорп. облигации" + Environment.NewLine +
                $"{newOverallShares:F}\t{newOverallGosBonds:F}\t{newOverallCorpBonds:F}" + Environment.NewLine +
                $"{newOverallShares / newOverall:P4}\t{newOverallGosBonds / newOverall:P4}\t{newOverallCorpBonds / newOverall:P4}"
                + Environment.NewLine + Environment.NewLine;
        }

        public string GetBuyMessage(BuyModel? model)
        {
            if (model == null)
                return "Не удалось вычислить";

            return $"{model.ShareCounts} акций, " +
                $"{model.GosBondCounts} гос. облигаций, " +
                $"{model.CorpBondCounts} корп. облигаций." + Environment.NewLine + Environment.NewLine +
                $"Отстаток средств: {model.SumDifference:F}." + Environment.NewLine +
                $"Отклонение по акциям: {model.SharePercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по гос. облигациям: {model.GosBondPercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по корп. облигациям: {model.CorpBondPercentDeviation:P8}";
        }

        private string GetPricesTable(StockPortfolioCalculationModel stockPortfolio)
        {
            var stockPortfolioPrices = _stockPortfolioService.GetStockProfilePrices(stockPortfolio);

            _stockPortfolioService.LoadPricesToModel(stockPortfolio, stockPortfolioPrices);

            var data = ToDataArrays(stockPortfolio.TickerInfos
                .Select(x => new PriceInfo
                {
                    Count = x.Count,
                    Price = stockPortfolioPrices[x.Ticker],
                    Ticker = x.Ticker,
                }));

            return TableFormatHelper.GetTable(data, 0, "|", needToMakeMonospaceFont: false);
        }

        private static string[][] ToDataArrays(IEnumerable<PriceInfo> models)
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
