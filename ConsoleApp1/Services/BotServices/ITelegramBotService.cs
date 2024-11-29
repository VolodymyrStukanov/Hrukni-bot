namespace HrukniBot.Services.BotServices
{
    public interface ITelegramBotService
    {
        Task StartBotAsync(CancellationToken cancellationToken);
    }
}