﻿using InvestCore.Domain.Models;

namespace InvestCore.Domain.Services.Interfaces
{
    public interface IShareService
    {
        [Obsolete($"Использовать GetCurrentOrLastPricesAsync")]
        Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<TickerInfoBase> tickerInfos);

        Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> tickerInfos);

        Task<decimal?> GetUSDRUBAsync();
    }
}
