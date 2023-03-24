using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ChatBot.Shares.Integration.Services
{
    public class TinkoffApiShareService : BaseShareService, IShareService
    {
        private readonly InvestCore.Domain.Services.Interfaces.IShareService _shareService;
        private readonly ILogger _logger;

        public TinkoffApiShareService(InvestCore.Domain.Services.Interfaces.IShareService shareService, ILogger logger)
        {
            _shareService = shareService;
            _logger = logger;
        }

        public override Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> symbols)
        {
            return _shareService.GetCurrentOrLastPricesAsync(symbols);
        }

        public override Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<TickerInfoBase> symbols)
        {
            return _shareService.GetPricesAsync(symbols);
        }
    }
}
