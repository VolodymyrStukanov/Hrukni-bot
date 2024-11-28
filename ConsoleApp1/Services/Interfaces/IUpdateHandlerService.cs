using Telegram.Bot.Types;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IUpdateHandlerService
    {
        Task HandleUpdate(Update update);
    }
}