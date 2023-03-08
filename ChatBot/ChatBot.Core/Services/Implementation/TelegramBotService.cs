using System.Data;
using System.Text.Json;
using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Timer = System.Timers.Timer;

namespace ChatBot.Core.Services.Implementation
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ITelegramBotClient _bot;
        private readonly Timer _timer;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IUserInfoService _userInfoService;
        private readonly ILogger _logger;

        public bool NeedSendMessagesOnStart { get; set; } = false;

        private readonly string _helpMessage;

        public TelegramBotService(string token,
            IUserService userService,
            IMessageService messageService,
            IUserInfoService chatService,
            ILogger logger,
            double messageInterval)
        {
            _bot = new TelegramBotClient(token);
            _timer = new Timer(messageInterval);
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            _timer.Elapsed += async (s, e) => await SendShareMessageInAllChatsAsync();
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            _userService = userService;
            _messageService = messageService;
            _userInfoService = chatService;
            _logger = logger;

            _helpMessage = $"You can control this bot by sending these commands: {Environment.NewLine}{Environment.NewLine}" +
                $"/help - get a list of commands{Environment.NewLine}/subscribe - subscribe on message distribution{Environment.NewLine}" +
                $"/unsubscribe - unsubscribe from message distribution{Environment.NewLine}" +
                $"/getmessage - get message now";
        }

        private async Task SendShareMessageInAllChatsAsync()
        {
            var infos = await _userInfoService.GetAllAsync();

            foreach (var info in infos.Where(x => x.IsSubscribed))
            {
                await SendOrUpdateShareMessageAsync(info.UserId);
            }
        }

        private async Task SendOrUpdateShareMessageAsync(long userId, bool canEditMessage = true)
        {
            var chat = new Chat()
            {
                Id = userId,
            };

            var userInfo = await _userInfoService.GetAsync(userId);

            if (userInfo == null)
            {
                return;
            }

            var message = await _messageService.GetMainTableMessageAsync(userInfo);

            await SendMessage(chat.Id, userInfo, message, canEditMessage);
        }

        private async Task SendMessage(long chatId, UserInfo userInfo, string message, bool canEditMessage)
        {
            if (userInfo.DataMessageId.HasValue && canEditMessage)
            {
                await EditMessage(chatId, userInfo.DataMessageId.Value, message);
            }
            else
            {
                await SendMessage(chatId, message, needToSaveMessageId: true);
            }
        }

        public async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bot start receiving at {datetime}", DateTime.Now);

            if (await _userInfoService.HasSubscribersAsync())
            {
                if (NeedSendMessagesOnStart)
                {
                    await SendShareMessageInAllChatsAsync();
                }

                _timer.Start();
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message },
            };

            _bot.StartReceiving(OnUpdateAsync, OnErrorAsync, receiverOptions, cancellationToken);
        }

        private async Task OnUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message ?? throw new NoNullAllowedException(nameof(update.Message));
            var user = message.From ?? throw new NoNullAllowedException(nameof(update.Message.From));
            _ = message.Text ?? throw new NoNullAllowedException(nameof(message.Text));

            if (!_userService.GetAllowedUserIds().Contains(user.Id))
            {
                LogUserAction(user, "tried to connect", LogLevel.Warning);
                await SendMessage(message.Chat.Id, "You don't have permission to use this bot", cancellationToken: cancellationToken);
                return;
            }

            var chatId = message.Chat.Id;
            switch (message.Text.ToLower())
            {
                case "/subscribe":
                    {
                        var userInfo = await _userInfoService.GetAsync(user.Id);
                        if (userInfo == null)
                        {
                            await SendMessage(message.Chat.Id, "You are not stored in database", cancellationToken: cancellationToken);
                            return;
                        }

                        if (userInfo.IsSubscribed)
                        {
                            await SendMessage(message.Chat.Id, "You already subscribed", cancellationToken: cancellationToken);
                            return;
                        }

                        userInfo.IsSubscribed = true;
                        await _userInfoService.UpdateAsync(userInfo);

                        await SendMessage(message.Chat.Id, "Subscribed successfully", cancellationToken: cancellationToken);

                        await SendOrUpdateShareMessageAsync(chatId);

                        LogUserAction(user, "subscribed");
                        _timer.Start();
                    }
                    break;

                case "/unsubscribe":
                    {
                        var userInfo = await _userInfoService.GetAsync(user.Id);
                        if (userInfo == null)
                        {
                            await SendMessage(message.Chat.Id, "You are not stored in database", cancellationToken: cancellationToken);
                            return;
                        }

                        if (!userInfo.IsSubscribed)
                        {
                            await SendMessage(message.Chat.Id, "You are not subscribed", cancellationToken: cancellationToken);
                            return;
                        }

                        userInfo.IsSubscribed = false;
                        await _userInfoService.UpdateAsync(userInfo);

                        await SendMessage(message.Chat.Id, "Unsubscribed successfully", cancellationToken: cancellationToken);

                        LogUserAction(user, "unsubscribed");
                        _timer.Stop();
                    }
                    break;

                case "/getmessage":
                    await SendOrUpdateShareMessageAsync(chatId, canEditMessage: false);
                    break;

                case "/start":
                case "/help":
                    {
                        await SendMessage(message.Chat.Id, _helpMessage, cancellationToken: cancellationToken);
                    }
                    break;

                #region Admin commands

                case "/userinfos":
                    {
                        var userInfo = await _userInfoService.GetAsync(user.Id);
                        if (userInfo != null && userInfo.IsAdmin)
                        {
                            var infos = await _userInfoService.GetAllAsync();
                            var msg = JsonSerializer.Serialize(infos);

                            await SendMessage(message.Chat.Id, msg, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            goto default;
                        }
                    }
                    break;

                //case "/stopbot":
                //    {
                //        var userInfo = await _userInfoService.GetAsync(user.Id);
                //        if (userInfo != null && userInfo.IsAdmin)
                //        {
                //            await SendMessage(message.Chat.Id, $"Stop bot requested at {DateTime.Now}", cancellationToken: cancellationToken);
                //
                //            await SendStoppedMessageAsync(cancellationToken);
                //
                //            throw new Exception("Application stopped");
                //        }
                //        else
                //        {
                //            goto default;
                //        }
                //    }

                #endregion Admin commands

                default:
                    {
                        await SendMessage(message.Chat.Id, "Unexpected command", cancellationToken: cancellationToken);
                        _logger.LogInformation("Recieved an unexpected message: \"{message}\"", message.Text);
                    }
                    break;
            }
        }

        private async Task SendMessage(long chatId, string message, bool needToSaveMessageId = false, CancellationToken cancellationToken = default)
        {
            var msg = await _bot.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

            await _userInfoService.SetDataMessageId(chatId,
                needToSaveMessageId
                    ? msg.MessageId
                    : null);

            _logger.LogInformation("Message sent to chat (Id = {chatId}): \"{message}\"", chatId, message);
        }

        private async Task EditMessage(long chatId, int messageId, string message)
        {
            await _bot.EditMessageTextAsync(chatId, messageId, message, parseMode: ParseMode.Markdown);

            _logger.LogInformation("Message edited in chat (Id = {chatId}): \"{message}\"", chatId, message);
        }

        private Task OnErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError("Error {message}", exception.Message);
            return Task.CompletedTask;
        }

        private void LogUserAction(User user, string message, LogLevel logLevel = LogLevel.Information)
        {
            _logger.Log(logLevel, "User {user} {message}", user, message);
        }

        public async Task SendStartedMessageAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bot started at {datetime}", DateTime.Now);

            var infos = await _userInfoService.GetAllAsync();
            var admin = infos.Where(x => x.IsAdmin).FirstOrDefault();

            if (admin != null)
            {
                await SendMessage(admin.UserId, $"Bot started at {DateTime.Now}", cancellationToken: cancellationToken);
            }
        }

        public async Task SendStoppedMessageAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bot stopped at {datetime}", DateTime.Now);

            var infos = await _userInfoService.GetAllAsync();
            var admin = infos.Where(x => x.IsAdmin).FirstOrDefault();

            if (admin != null)
            {
                await SendMessage(admin.UserId, $"Bot stopped at {DateTime.Now}", cancellationToken: cancellationToken);
            }
        }
    }
}
