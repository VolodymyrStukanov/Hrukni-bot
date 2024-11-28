
using Telegram.Bot.Types;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IFilesService
    {
        public void WriteObjectToJSON(string fullPath, object obj);
        public void WriteErrorUpdate(Update update);
        public void WriteUpdate(Update update);
    }
}
