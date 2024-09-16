namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface ITelegramBotService
    {
        Task StartBotAsync(CancellationToken cancellationToken);
    }
}