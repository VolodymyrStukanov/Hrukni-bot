using Telegram.Bot.Types;

namespace HrukniBot.Services.FilesService
{
    public interface IFilesService
    {
        public void WriteObjectToJSON(string fullPath, object obj);
        public void WriteErrorUpdate(Update update);
        public void WriteUpdate(Update update);
    }
}
