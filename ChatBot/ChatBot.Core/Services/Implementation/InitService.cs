using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ChatBot.Core.Services.Implementation
{
    public class InitService : IInitService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly ILogger _logger;

        public InitService(IUserInfoService userInfoService, ILogger logger)
        {
            _userInfoService = userInfoService;
            _logger = logger;
        }

        public async Task Init()
        {
            if ((await _userInfoService.GetAllAsync()).Any())
            {
                return;
            }

            _logger.LogInformation("First initialization");

            await _userInfoService.AddAsync(new UserInfo(1485723680, isAdmin: true, new TickerInfoModel[]
                    {
                        new("SBER", "SBER", 80, InstrumentType.Share),
                        new("SNGSP", "SNGSP", 1000, InstrumentType.Share),
                        new("MTSS", "MTSS", 100, InstrumentType.Share),
                        new("MGNT", "MGNT", 4, InstrumentType.Share),
                        new("OGKB", "OGKB", 40000, InstrumentType.Share),
                        new("LKOH", "LKOH", 6, InstrumentType.Share),
                        new("RASP", "RASP", 30, InstrumentType.Share),
                        new("MAGN", "MAGN", 200, InstrumentType.Share),
                        new("PHOR", "PHOR", 1, InstrumentType.Share),
                        new("RU000A103WV8", "Sb33r", 30, InstrumentType.Bond),
                    })
            {
                IsSubscribed = false,
            });

            await _userInfoService.AddAsync(new UserInfo(1267626025, isAdmin: false, new TickerInfoModel[]
                    {
                        new("GAZP", "GAZP", 100, InstrumentType.Share),
                        new("SBER", "SBER", 20, InstrumentType.Share),
                        new("ROSN", "ROSN", 80, InstrumentType.Share),
                        new("NVTK", "NVTK", 6, InstrumentType.Share),
                    })
            {
                IsSubscribed = false,
            });
        }
    }
}
