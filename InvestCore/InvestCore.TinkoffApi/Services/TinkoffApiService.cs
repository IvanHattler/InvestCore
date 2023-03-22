using System.Net.Http.Headers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.TinkoffApi.Domain;
using InvestCore.TinkoffApi.Infrastructure;
using Microsoft.Extensions.Logging;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using InstrumentType = InvestCore.Domain.Models.InstrumentType;

namespace InvestCore.TinkoffApi.Services
{
    public class TinkoffApiService : IShareService
    {
        private readonly InvestApiClient _investApiClient;
        private readonly ILogger _logger;
        const string Usd = "usd";
        private decimal? USDRUB;
        private IEnumerable<Share> Shares;
        private IEnumerable<Bond> Bonds;
        private IEnumerable<Etf> Etfs;

        public TinkoffApiService(InvestApiClient investApiClient, ILogger logger)
        {
            _investApiClient = investApiClient;
            _logger = logger;
        }

        public async Task<decimal?> GetUSDRUBAsync()
        {
            try
            {
                //BBG0013HGFT4 USD000UTSTOM
                return USDRUB ??= await GetByCandles("BBG0013HGFT4")
                    ?? await GetClosePriceByCandles("BBG0013HGFT4");
            }
            finally
            {
                _logger.LogInformation("Выполнен запрос к TinkoffApi");
            }
        }
        protected async Task<IEnumerable<Share>> GetSharesAsync()
        {
            if (Shares == null)
            {
                try
                {
                    Shares = (await _investApiClient.Instruments.SharesAsync()).Instruments;
                }
                finally
                {
                    _logger.LogInformation("Выполнен запрос к TinkoffApi");
                }
            }
            return Shares;
        }
        protected async Task<IEnumerable<Bond>> GetBondsAsync()
        {
            if (Bonds == null)
            {
                try
                {
                    Bonds = (await _investApiClient.Instruments.BondsAsync()).Instruments;
                }
                finally
                {
                    _logger.LogInformation("Выполнен запрос к TinkoffApi");
                }
            }
            return Bonds;

        }
        protected async Task<IEnumerable<Etf>> GetEtfsAsync()
        {
            if (Etfs == null)
            {
                try
                {
                    Etfs = (await _investApiClient.Instruments.EtfsAsync()).Instruments;
                }
                finally
                {
                    _logger.LogInformation("Выполнен запрос к TinkoffApi");
                }
            }
            return Etfs;
        }

        public async Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<TickerInfoBase> tickerInfos)
        {
            var result = new Dictionary<string, decimal>(tickerInfos.Count());
            try
            {
                foreach (var tickerInfo in tickerInfos)
                {
                    var currentPrice = await GetCurrentPrice(tickerInfo.Ticker, tickerInfo.TickerType, tickerInfo.ClassCode);

                    if (currentPrice.HasValue)
                    {
                        result.TryAdd(tickerInfo.Ticker, currentPrice.Value);
                    }
                }
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unauthenticated)
                {
                    _logger.LogCritical("Tinkoff token is irrelevant");
                }
                if (e.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogCritical("Ticker not found");
                }
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> tickerInfos)
        {
            var result = new Dictionary<string, decimal>(tickerInfos.Count());
            var symbolModels = await GetSymbolModels(tickerInfos);

            foreach (var symbolModel in symbolModels)
            {
                try
                {
                    var currentPrice = await GetCurrentOrLastPrice(symbolModel);

                    if (currentPrice.HasValue)
                    {
                        if (symbolModel.Currency == Usd)
                            currentPrice *= await GetUSDRUBAsync();

                        result.TryAdd(symbolModel.Symbol, currentPrice.Value);
                    }
                }
                catch (InstrumentNotFoundException e)
                {
                    _logger.LogCritical("Ticker {ticker} not found", e.Message);
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Unauthenticated)
                    {
                        _logger.LogCritical("Tinkoff token is irrelevant");
                    }
                    if (e.StatusCode == StatusCode.NotFound)
                    {
                        _logger.LogCritical("Ticker {ticker} not found", symbolModel.Symbol);
                    }
                }
            }

            FillDefaultValues(tickerInfos, result);

            return result;
        }

        private void FillDefaultValues(IEnumerable<TickerInfoBase> tickerInfos, Dictionary<string, decimal> result)
        {
            if (result.Count < tickerInfos.Count())
            {
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
                var defaultPrices = tickerInfos
                    .Where(x => x.DefaultPrice.HasValue)
                    .ToDictionary(x => x.Ticker, x => x.DefaultPrice.Value);
#pragma warning restore CS8629 // Тип значения, допускающего NULL, может быть NULL.

                foreach (var tickerInfo in tickerInfos.Where(x => !result.ContainsKey(x.Ticker)))
                {
                    if (defaultPrices.ContainsKey(tickerInfo.Ticker))
                    {
                        result.TryAdd(tickerInfo.Ticker, defaultPrices[tickerInfo.Ticker]);
                        _logger.LogWarning("Used default price for {symbol}", tickerInfo.Ticker);
                    }
                }
            }
        }

        private async Task<IEnumerable<SymbolModel>> GetSymbolModels(IEnumerable<TickerInfoBase> tickerInfos)
        {
            var res = new List<SymbolModel>();

            foreach (var tickerInfo in tickerInfos)
            {
                try
                {
                    var figiAndNominal = await GetDataForPrice(tickerInfo.Ticker, tickerInfo.TickerType, tickerInfo.ClassCode);
                    res.Add(new SymbolModel()
                    {
                        Symbol = tickerInfo.Ticker,
                        Type = tickerInfo.TickerType,
                        Figi = figiAndNominal.Item1,
                        Nominal = figiAndNominal.Item2,
                        Currency = figiAndNominal.Item3,
                    });
                }
                catch (InstrumentNotFoundException e)
                {
                    _logger.LogCritical("Ticker {ticker} not found", e.Message);
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Unauthenticated)
                    {
                        _logger.LogCritical("Tinkoff token is irrelevant");
                    }
                    if (e.StatusCode == StatusCode.NotFound)
                    {
                        _logger.LogCritical("Ticker {ticker} not found", tickerInfo.Ticker);
                    }
                }
            }

            return res;
        }

        private async Task<(string, MoneyValue?, string)> GetDataForPrice(string ticker, InstrumentType type, string classCode)
        {
            switch (type)
            {
                case InstrumentType.Share:
                    {
                        var share = (await GetSharesAsync())
                            .Where(x => x.Ticker == ticker && x.ClassCode == classCode)
                            .FirstOrDefault()
                            ?? throw new InstrumentNotFoundException(ticker);

                        return (share.Figi, null, share.Currency);
                    }
                case InstrumentType.Bond:
                    {
                        var bond = (await GetBondsAsync())
                            .Where(x => x.Ticker == ticker && x.ClassCode == classCode)
                            .FirstOrDefault()
                            ?? throw new InstrumentNotFoundException(ticker);

                        return (bond.Figi, bond.Nominal, bond.Currency);
                    }
                case InstrumentType.Etf:
                    {
                        var etf = (await GetEtfsAsync())
                            .Where(x => x.Ticker == ticker && x.ClassCode == classCode)
                            .FirstOrDefault()
                            ?? throw new InstrumentNotFoundException(ticker);

                        return (etf.Figi, null, etf.Currency);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<decimal?> GetCurrentPrice(string symbol, InstrumentType type, string classCode)
        {
            try
            {
                switch (type)
                {
                    case InstrumentType.Share:
                        {
                            var share = (await _investApiClient.Instruments.ShareByAsync(
                                            new InstrumentRequest()
                                            {
                                                IdType = InstrumentIdType.Ticker,
                                                ClassCode = classCode,
                                                Id = symbol,
                                            }
                                        )).Instrument;

                            return await GetByCandles(share.Figi);
                        }
                    case InstrumentType.Bond:
                        {
                            var bond = (await _investApiClient.Instruments.BondByAsync(
                                            new InstrumentRequest()
                                            {
                                                IdType = InstrumentIdType.Ticker,
                                                ClassCode = classCode,
                                                Id = symbol,
                                            }
                                        )).Instrument;

                            var price = await GetByCandles(bond.Figi);

                            if (price == null)
                                return null;

                            return await CalculateBondPrice(bond.Figi, bond.Nominal, price.Value);
                        }
                    case InstrumentType.Etf:
                        {
                            var etf = (await _investApiClient.Instruments.EtfByAsync(
                                            new InstrumentRequest()
                                            {
                                                IdType = InstrumentIdType.Ticker,
                                                ClassCode = classCode,
                                                Id = symbol,

                                            }
                                        )).Instrument;

                            return await GetByCandles(etf.Figi);
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                _logger.LogInformation("Выполнен запрос к TinkoffApi");
            }
        }

        private async Task<decimal?> GetCurrentOrLastPrice(SymbolModel model)
        {
            try
            {
                switch (model.Type)
                {
                    case InstrumentType.Share:
                    case InstrumentType.Etf:
                        {
                            return await GetByCandles(model.Figi)
                                ?? await GetClosePriceByCandles(model.Figi);
                        }
                    case InstrumentType.Bond:
                        {
                            if (model.Nominal == null)
                                return null;

                            var price = await GetByCandles(model.Figi)
                                ?? await GetClosePriceByCandles(model.Figi);

                            if (price == null)
                                return null;

                            return await CalculateBondPrice(model.Figi, model.Nominal, price.Value);
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                _logger.LogInformation("Выполнен запрос к TinkoffApi");
            }
        }

        private async Task<decimal?> CalculateBondPrice(string figi, MoneyValue nominal, decimal price)
        {
            var accruedInterests = (await _investApiClient.Instruments.GetAccruedInterestsAsync(new GetAccruedInterestsRequest
            {
                Figi = figi,
                From = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                To = Timestamp.FromDateTime(DateTime.UtcNow)
            })).AccruedInterests;

            var accruedInterest = accruedInterests
                .OrderBy(x => x.Date)
                .Last()
                .Value;

            return price / 100 * nominal + accruedInterest;
        }

        private async Task<decimal?> GetByCandles(string figi)
        {
            var response = await _investApiClient.MarketData.GetCandlesAsync(new GetCandlesRequest()
            {
                From = DateTime.UtcNow.AddMinutes(-10).ToTimestamp(),
                To = DateTime.UtcNow.ToUniversalTime().ToTimestamp(),
                Figi = figi,
                Interval = CandleInterval._1Min,
            });

            var lastCandle = response.Candles
                .OrderBy(x => x.Time)
                .LastOrDefault();

            if (lastCandle != null)
                return CalculatePriceByCandle(lastCandle);

            return null;
        }

        private static decimal CalculatePriceByCandle(HistoricCandle candle)
        {
            return candle.Open;
        }

        private async Task<decimal?> GetClosePriceByCandles(string figi)
        {
            var response = await _investApiClient.MarketData.GetCandlesAsync(new GetCandlesRequest()
            {
                From = DateTime.UtcNow.AddDays(-3).ToTimestamp(),
                To = DateTime.UtcNow.ToUniversalTime().ToTimestamp(),
                Figi = figi,
                Interval = CandleInterval.Day,
            });

            var lastCandle = response.Candles
                .OrderBy(x => x.Time)
                .LastOrDefault();

            if (lastCandle != null)
                return lastCandle.Close;

            return null;
        }
    }
}
