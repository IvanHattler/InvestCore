using PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IBuyModelService
    {
        BuyModel? CalculateBestBuyModel(InstrumentCalculationModel shareDto, InstrumentCalculationModel gosBondDto,
            InstrumentCalculationModel corpBondDto, ReplenishmentModel replenishmentDto,
            double stepPercent = .1);
    }
}
