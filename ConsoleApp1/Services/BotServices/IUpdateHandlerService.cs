using Telegram.Bot.Types;

namespace HrukniBot.Services.BotServices
{
    public interface IUpdateHandlerService
    {
        Task HandleUpdate(Update update);
    }
}