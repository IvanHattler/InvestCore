using InvestCore.PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Services.Interfaces
{
    public interface ISelectBestBuyModelStrategy
    {
        BuyModel? SelectBestModel(List<BuyModel> buyModels);
    }
}
