using InvestCore.PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Services.Interfaces
{
    public interface IBuyModelService
    {
        BuyModel? CalculateBestBuyModel(InstrumentCalculationModel shareDto, InstrumentCalculationModel gosBondDto, InstrumentCalculationModel corpBondDto, ReplenishmentModel replenishmentDto);
    }
}
