using InvestCore.PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Services.Interfaces
{
    public interface IBuyModelService
    {
        BuyModel? CalculateBestBuyModelOptimized(InstrumentCalculationModel shareDto, InstrumentCalculationModel gosBondDto, InstrumentCalculationModel corpBondDto, ReplenishmentModel replenishmentDto);
    }
}
