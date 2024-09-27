using HrukniHohlinaBot.Services.BotServices;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace HrukniHohlinaBot.Services.FilesService
{
    public class FilesService : IFilesService
    {
        private readonly ILogger<TelegramBotService> _logger;

        private string? _updateStorageLocation;
        private string? _updateStorageFileNamePattern;
        private string? _errorUpdateStorageLocation;
        private string? _errorUpdateStorageFileNamePattern;
        private string? _currentDir;
        public FilesService(IConfiguration configuration, ILogger<TelegramBotService> logger)
        {

            var workingDirectory = Directory.GetParent(Environment.CurrentDirectory);
            if (workingDirectory != null)
#if Docker
                _currentDir = workingDirectory.FullName;
#else
                _currentDir = workingDirectory.Parent.Parent.FullName;
#endif
            else
                throw new Exception("Can`t open the project directory");

            _logger = logger;
            _updateStorageLocation = configuration.GetSection("UpdatesStorage").GetValue<string>("FolderName");
            _updateStorageFileNamePattern = configuration.GetSection("UpdatesStorage").GetValue<string>("FileNamePattern");
            _errorUpdateStorageLocation = configuration.GetSection("ErrorUpdatesStorage").GetValue<string>("FolderName");
            _errorUpdateStorageFileNamePattern = configuration.GetSection("ErrorUpdatesStorage").GetValue<string>("FileNamePattern");
        }

        public void WriteObjectToJSON(string fullPath, object obj)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    using (var filestream = File.Create(fullPath))
                    {

                        var text = Encoding.ASCII.GetBytes(System.Text.Json.JsonSerializer.Serialize(obj));
                        filestream.Write(text);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in WriteObjectToJSON method: {ex.Message}");
            }
        }

        public void WriteErrorUpdate(Update update)
        {
            var folderPath = Path.Combine(_currentDir, _updateStorageLocation);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            WriteObjectToJSON(
                Path.Combine(_currentDir, 
                    _errorUpdateStorageLocation,
                    string.Format(_errorUpdateStorageFileNamePattern, DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss.ffff"))), 
                update);
        }

        public void WriteUpdate(Update update)
        {
            var folderPath = Path.Combine(_currentDir, _updateStorageLocation);
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var datetime = DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss.ffff");
            var fullPath = Path.Combine(folderPath, string.Format(_updateStorageFileNamePattern, datetime));

            WriteObjectToJSON(fullPath, update);
        }
    }
}
