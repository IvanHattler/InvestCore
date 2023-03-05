using PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IMetricStrategy
    {
        decimal GetMetric(BuyModel model);
    }
}
