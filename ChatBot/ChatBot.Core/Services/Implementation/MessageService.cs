using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Helpers;
using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Implementation
{
    public class MessageService : IMessageService
    {
        private readonly IShareService _shareService;
        private readonly IUserInfoService _userInfoService;

        public MessageService(IShareService shareService, IUserInfoService userInfoService)
        {
            _shareService = shareService;
            _userInfoService = userInfoService;
        }

        public async Task<string> GetMainTableMessageAsync(UserInfo userInfo)
        {
            await _shareService.UpdateTickerInfosAsync(userInfo.TickerInfos);

            await _userInfoService.UpdateAsync(userInfo);

            string[][] data = ToDataArrays(userInfo.TickerInfos);

            return $"Shares quotes on {DateTime.Now}{Environment.NewLine}{Environment.NewLine}"
                + TableFormatHelper.GetTable(data, 0, "|");
        }

        private static string[][] ToDataArrays(IEnumerable<TickerInfoModel> models)
        {
            var data = new string[models.Count() + 2][];
            int i = 0;
            data[i++] = new[] { "№", "Ticker", "Count", "Price", "Value", "Change" };

            foreach (var model in models.OrderByDescending(x => x.Value))
            {
                data[i] = new[] {
                    $"{i++}",
                    model.DisplayName,
                    model.Count.ToString(),
                    TableFormatHelper.GetFormattedNumber(model.Price),
                    TableFormatHelper.GetFormattedNumber(model.Value),
                    $"{TableFormatHelper.GetFormattedNumber(model.Difference)} ({model.DifferencePercent:P2})"
                };
            }

            var valueOverall = models.Sum(x => x.Value);
            var differenceOverall = models.Sum(x => x.Difference);
            var differencePercentOverall = valueOverall - differenceOverall != 0
                ? differenceOverall / (valueOverall - differenceOverall)
                : 1;

            data[i] = new[]
            {
                "",
                "",
                "",
                "",
                TableFormatHelper.GetFormattedNumber(valueOverall),
                $"{TableFormatHelper.GetFormattedNumber(differenceOverall)} ({differencePercentOverall:P2})"
            };

            return data;
        }
    }
}
