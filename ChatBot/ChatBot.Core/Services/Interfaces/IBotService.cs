namespace ChatBot.Core.Services.Interfaces
{
    public interface ITelegramBotService
    {
        Task StartReceivingAsync(CancellationToken cancellationToken);

        Task SendStartedMessageAsync(CancellationToken cancellationToken);

        Task SendStoppedMessageAsync(CancellationToken cancellationToken);
    }
}
