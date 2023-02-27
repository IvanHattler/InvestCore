using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvestCore.Domain.Services.Interfaces;
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
            //todo: service for download dictionaries with instruments data
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

        private async Task<decimal?> GetCurrentPrice(string symbol, InstrumentType type)
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

                        return await CalculateBondPrice(bond);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<decimal?> CalculateBondPrice(Bond bond)
        {
            var price = await GetByCandles(bond.Figi);

            var accruedInterests = (await _investApiClient.Instruments.GetAccruedInterestsAsync(new GetAccruedInterestsRequest
            {
                Figi = bond.Figi,
                From = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                To = Timestamp.FromDateTime(DateTime.UtcNow)
            })).AccruedInterests;

            var accruedInterest = accruedInterests
                .OrderBy(x => x.Date)
                .Last()
                .Value;

            if (price == null)
                return null;

            return price / 100 * bond.Nominal + accruedInterest;
        }

        private async Task<decimal?> GetByCandles(string figi)
        {
            var candles = await _investApiClient.MarketData.GetCandlesAsync(new GetCandlesRequest()
            {
                From = DateTime.UtcNow.AddMinutes(-10).ToTimestamp(),
                To = DateTime.UtcNow.ToUniversalTime().ToTimestamp(),
                Figi = figi,
                Interval = CandleInterval._1Min,
            });

            var lastCandle = candles.Candles
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
    }
}
