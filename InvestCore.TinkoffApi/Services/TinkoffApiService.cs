using System.Linq.Expressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.TinkoffApi.Domain;
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

        public TinkoffApiService(InvestApiClient investApiClient, ILogger logger)
        {
            _investApiClient = investApiClient;
            _logger = logger;
        }

        public async Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<(string, InstrumentType)> symbols)
        {
            var result = new Dictionary<string, decimal>(symbols.Count());
            try
            {
                foreach (var (symbol, type) in symbols)
                {
                    var currentPrice = await GetCurrentPrice(symbol, type);

                    if (currentPrice.HasValue)
                    {
                        result.TryAdd(symbol, currentPrice.Value);
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

        public async Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<(string, InstrumentType)> symbols)
        {
            var result = new Dictionary<string, decimal>(symbols.Count());
            try
            {
                var symbolModels = await GetSymbolModels(symbols);

                foreach (var symbolModel in symbolModels)
                {
                    var currentPrice = await GetCurrentOrLastPrice(symbolModel.Figi, symbolModel.Type, symbolModel.Nominal);

                    if (currentPrice.HasValue)
                    {
                        result.TryAdd(symbolModel.Symbol, currentPrice.Value);
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

        private async Task<IEnumerable<SymbolModel>> GetSymbolModels(IEnumerable<(string, InstrumentType)> symbols)
        {
            var res = new List<SymbolModel>();

            foreach (var (symbol, type) in symbols)
            {
                var figiAndNominal = await GetFigiAndNominal(symbol, type);
                res.Add(new SymbolModel()
                {
                    Symbol = symbol,
                    Type = type,
                    Figi = figiAndNominal.Item1,
                    Nominal = figiAndNominal.Item2,
                });
            }

            return res;
        }

        private async Task<(string, MoneyValue?)> GetFigiAndNominal(string? symbol, InstrumentType type)
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
                                                 ClassCode = "TQBR",
                                                 Id = symbol,
                                             })).Instrument;

                             return (share.Figi, null);
                        }
                    case InstrumentType.Bond:
                        {
                            var bond = (await _investApiClient.Instruments.BondByAsync(
                                            new InstrumentRequest()
                                            {
                                                IdType = InstrumentIdType.Ticker,
                                                ClassCode = "TQCB",
                                                Id = symbol,
                                            }
                                        )).Instrument;

                            return (bond.Figi, bond.Nominal);
                        }
                    case InstrumentType.Etf:
                        {
                            var etf = (await _investApiClient.Instruments.EtfByAsync(
                                            new InstrumentRequest()
                                            {
                                                IdType = InstrumentIdType.Ticker,
                                                ClassCode = "TQTF",
                                                Id = symbol,
                                            }
                                        )).Instrument;

                            return (etf.Figi, null);
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

        private async Task<decimal?> GetCurrentPrice(string symbol, InstrumentType type)
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
                                                ClassCode = "TQBR",
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
                                                ClassCode = "TQCB",
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
                                                ClassCode = "TQTF",
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

        private async Task<decimal?> GetCurrentOrLastPrice(string figi, InstrumentType type, MoneyValue? nominal)
        {
            try
            {
                switch (type)
                {
                    case InstrumentType.Share:
                    case InstrumentType.Etf:
                        {
                            return await GetByCandles(figi)
                                ?? await GetClosePriceByCandles(figi);
                        }
                    case InstrumentType.Bond:
                        {
                            var price = await GetByCandles(figi)
                                ?? await GetClosePriceByCandles(figi);

                            if (price == null)
                                return null;

                            return await CalculateBondPrice(figi, nominal, price.Value);
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

        private decimal CalculatePriceByCandle(HistoricCandle candle)
        {
            return candle.Open;
        }

        private async Task<decimal?> GetClosePriceByCandles(string figi)
        {
            var response = await _investApiClient.MarketData.GetCandlesAsync(new GetCandlesRequest()
            {
                From = DateTime.UtcNow.AddDays(-1).ToTimestamp(),
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
