using ChatBot.Core.Services.Interfaces;

namespace ChatBotWorker
{
    public class ChatBotWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ITelegramBotService _telegramBotService;
        private readonly IInitService _initService;

        public ChatBotWorker(ILogger logger, ITelegramBotService telegramBotService, IInitService initService)
        {
            _logger = logger;
            _telegramBotService = telegramBotService;
            _initService = initService;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _telegramBotService.SendStoppedMessageAsync(cancellationToken);
            }
            catch (Exception)
            {
            }

            await base.StopAsync(cancellationToken);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _telegramBotService.SendStartedMessageAsync(cancellationToken);
            }
            catch (Exception)
            {
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _initService.Init();
                await _telegramBotService.StartReceivingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }
    }
}
