using System.Text;
using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Interfaces;

namespace InvestCore.PercentCalculateConsole.Services.Implementation
{
    public class MessageService : IMessageService
    {
        private readonly IStockPortfolioService _stockPortfolioService;
        private readonly IBuyModelService _buyModelService;

        public MessageService(IStockPortfolioService stockPortfolioService, IBuyModelService buyModelService)
        {
            _stockPortfolioService = stockPortfolioService;
            _buyModelService = buyModelService;
        }

        public string GetResultMessage(StockPortfolioCalculationModel stockPortfolio)
        {
            _stockPortfolioService.LoadPricesToModel(stockPortfolio);

            var sb = new StringBuilder();
            sb.AppendLine("--------------------------Месяц №0--------------------------");
            sb.AppendLine(GetOverallMessage(stockPortfolio));

            for (int i = 0; i < 3; i++)
            {
                var bestModel = _buyModelService.CalculateBestBuyModel(stockPortfolio.Share,
                    stockPortfolio.GosBond, stockPortfolio.CorpBond, stockPortfolio.Replenishment);

                sb.AppendLine($"--------------------------Месяц №{i + 1}--------------------------");
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
    }
}
