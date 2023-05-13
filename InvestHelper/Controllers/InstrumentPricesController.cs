using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvestHelper.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class InstrumentPricesController : ControllerBase
    {
        private readonly IShareService _shareService;

        public InstrumentPricesController(IShareService shareService)
        {
            _shareService = shareService ?? throw new ArgumentNullException(nameof(shareService));
        }

        [HttpPost]
        public async Task<IActionResult> GetPricesAsync(IEnumerable<TickerInfoDto> tickerInfos)
        {
            if (!Validate(tickerInfos))
                return BadRequest("Переданы некорретные данные");

            return Ok(await _shareService.GetCurrentOrLastPricesAsync(
                tickerInfos.Select(x => new TickerInfoBase()
                {
                    Ticker = x.Ticker,
                    ClassCode = x.ClassCode,
                    TickerType = x.TickerType,
                })));
        }

        private bool Validate(IEnumerable<TickerInfoDto> tickerInfos)
        {
            var allowedTickerTypes = new InstrumentType[]
            {
                InstrumentType.Share,
                InstrumentType.Bond,
                InstrumentType.Etf,
            };

            return tickerInfos.All(t => !string.IsNullOrEmpty(t.Ticker)
                && !string.IsNullOrEmpty(t.ClassCode)
                && allowedTickerTypes.Contains(t.TickerType));
        }
    }
}
