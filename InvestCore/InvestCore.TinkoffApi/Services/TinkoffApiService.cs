using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.TinkoffApi.Domain;
using InvestCore.TinkoffApi.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private const string Usd = "usd";
        private decimal? USDRUB;
        private IEnumerable<Share> Shares;
        private IEnumerable<Bond> Bonds;
        private IEnumerable<Etf> Etfs;
        private TimeSpan cacheTime = TimeSpan.FromMinutes(5);

        public TinkoffApiService(InvestApiClient investApiClient, ILogger logger, IMemoryCache memoryCache)
        {
            _investApiClient = investApiClient;
            _logger = logger;
            _memoryCache = memoryCache;

            Preload();
        }

        public void Preload()
        {
            Task.WaitAll(
                GetUSDRUBAsync(),
                GetSharesAsync(),
                GetBondsAsync(),
                GetEtfsAsync());
        }

        public async Task<decimal?> GetUSDRUBAsync()
        {
            if (USDRUB == null)
            {
                try
                {
                    //BBG0013HGFT4 USD000UTSTOM
                    USDRUB = await GetClosePrice("BBG0013HGFT4").ConfigureAwait(false);
                }
                finally
                {
                    LogRequestCompleted();
                }
            }
            return USDRUB;
        }

        protected async Task<IEnumerable<Share>> GetSharesAsync()
        {
            if (Shares == null)
            {
                try
                {
                    Shares = (await _investApiClient.Instruments.SharesAsync().ConfigureAwait(false)).Instruments;
                }
                finally
                {
                    LogRequestCompleted();
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
                    Bonds = (await _investApiClient.Instruments.BondsAsync().ConfigureAwait(false)).Instruments;
                }
                finally
                {
                    LogRequestCompleted();
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
                    Etfs = (await _investApiClient.Instruments.EtfsAsync().ConfigureAwait(false)).Instruments;
                }
                finally
                {
                    LogRequestCompleted();
                }
            }
            return Etfs;
        }

        [Obsolete($"Использовать GetCurrentOrLastPricesAsync")]
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
                    _logger.LogError("Tinkoff token is irrelevant");
                }
                if (e.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogWarning("Ticker not found");
                }
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> tickerInfos)
        {
            var tasks = tickerInfos
                .Select(t => AwaitAndGetPriceAsync(
                    GetDataForPrice(t)))
                .ToArray();

            var res = await Task.WhenAll(tasks);

            var result = ToDictionary(res);
            FillDefaultValues(tickerInfos, result);

            return result;
        }

        private async Task<(string, decimal)?> AwaitAndGetPriceAsync(Task<SymbolModel?> task)
        {
            var symbolModel = await task.ConfigureAwait(false);
            if (symbolModel != null)
                return await GetPriceAsync(symbolModel).ConfigureAwait(false);

            return null;
        }

        private static Dictionary<string, decimal> ToDictionary((string, decimal)?[] res)
        {
            if (res == null)
                return new Dictionary<string, decimal>(0);

            var result = new Dictionary<string, decimal>(res.Length);

            foreach (var r in res.Where(x => x.HasValue))
            {
                result.TryAdd(r.Value.Item1, r.Value.Item2);
            }

            return result;
        }

        private async Task<(string, decimal)?> GetPriceAsync(SymbolModel symbolModel)
        {
            try
            {
                var currentPrice = await GetLastPrice(symbolModel).ConfigureAwait(false);

                if (currentPrice.HasValue)
                {
                    if (symbolModel.Currency == Usd)
                        currentPrice *= await GetUSDRUBAsync().ConfigureAwait(false);

                    return (symbolModel.Symbol, currentPrice.Value);
                }
            }
            catch (InstrumentNotFoundException e)
            {
                _logger.LogWarning("Ticker {ticker} not found", e.Message);
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unauthenticated)
                {
                    _logger.LogError("Tinkoff token is irrelevant");
                }
                if (e.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogWarning("Ticker {ticker} not found", symbolModel.Symbol);
                }
            }

            return null;
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

        private async Task<SymbolModel?> GetDataForPrice(TickerInfoBase tickerInfo)
        {
            try
            {
                var dataForPrice = await GetDataForPrice(tickerInfo.Ticker, tickerInfo.TickerType, tickerInfo.ClassCode).ConfigureAwait(false);

                return new SymbolModel()
                {
                    Symbol = tickerInfo.Ticker,
                    Type = tickerInfo.TickerType,
                    Figi = dataForPrice.Item1,
                    Nominal = dataForPrice.Item2,
                    Currency = dataForPrice.Item3,
                };
            }
            catch (InstrumentNotFoundException e)
            {
                _logger.LogWarning("Ticker {ticker} not found", e.Message);
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unauthenticated)
                {
                    _logger.LogError("Tinkoff token is irrelevant");
                }
                if (e.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogWarning("Ticker {ticker} not found", tickerInfo.Ticker);
                }
            }

            return null;
        }

        private async Task<(string, decimal?, string)> GetDataForPrice(string ticker, InstrumentType type, string classCode)
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

        [Obsolete]
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
                LogRequestCompleted();
            }
        }

        private static string GetCacheKey(SymbolModel model)
            => model.Figi;

        private Task<decimal?> GetLastPrice(SymbolModel model)
        {
            string key = GetCacheKey(model);

            return _memoryCache.GetOrCreateAsync(key, GetLastPrice);

            async Task<decimal?> GetLastPrice(ICacheEntry entry)
            {
                entry.SetAbsoluteExpiration(cacheTime);
                return await GetLastPriceInternal(model);
            }
        }

        private async Task<decimal?> GetLastPriceInternal(SymbolModel model)
        {
            try
            {
                switch (model.Type)
                {
                    case InstrumentType.Share:
                    case InstrumentType.Etf:
                        {
                            return await GetClosePrice(model.Figi);
                        }
                    case InstrumentType.Bond:
                        {
                            if (model.Nominal == null)
                                return null;

                            var price = await GetClosePrice(model.Figi);

                            if (price == null)
                                return null;

                            return await CalculateBondPrice(model.Figi, model.Nominal.Value, price.Value);
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                LogRequestCompleted();
            }
        }

        private void LogRequestCompleted()
        {
            _logger.LogInformation("TinkoffApi request completed");
        }

        private async Task<decimal?> CalculateBondPrice(string figi, decimal nominal, decimal price)
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

        [Obsolete]
        private async Task<decimal?> GetByCandles(string figi)
        {
            var response = await _investApiClient.MarketData.GetCandlesAsync(new GetCandlesRequest()
            {
                From = DateTime.UtcNow.AddMinutes(-10).ToTimestamp(),
                To = DateTime.UtcNow.ToUniversalTime().ToTimestamp(),
                InstrumentId = figi,
                Interval = CandleInterval._1Min,
            });

            var lastCandle = response
                .Candles
                .OrderBy(x => x.Time)
                .LastOrDefault();

            if (lastCandle != null)
                return CalculatePriceByCandle(lastCandle);

            return null;
        }

        [Obsolete]
        private static decimal CalculatePriceByCandle(HistoricCandle candle)
        {
            return candle.Open;
        }

        private async Task<decimal?> GetClosePrice(string figi)
        {
            var request = new GetClosePricesRequest();
            request.Instruments.Add(new InstrumentClosePriceRequest() { InstrumentId = figi });

            var response = await _investApiClient.MarketData.GetClosePricesAsync(request);

            return response
                .ClosePrices
                .First()
                .Price;
        }
    }
}
