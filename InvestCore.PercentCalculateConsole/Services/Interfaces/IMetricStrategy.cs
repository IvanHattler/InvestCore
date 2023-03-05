using InvestCore.PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Services.Interfaces
{
    public interface IMetricStrategy
    {
        decimal GetMetric(BuyModel model);
    }
}
