using InvestCore.Domain.Models;
using PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IBuyModelService
    {
        BuyModel? CalculateBestBuyModel(InstrumentCalculationModel shareDto, InstrumentCalculationModel gosBondDto,
            InstrumentCalculationModel corpBondDto, InstrumentCalculationModel gold, ReplenishmentModel replenishmentDto,
            double stepPercent = .1);
    }
}
