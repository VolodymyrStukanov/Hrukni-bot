using HrukniBot.Services.FilesService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace HrukniHohlinaBot.Services.FilesService
{
    public class FilesService : IFilesService
    {
        private readonly ILogger<FilesService> logger;

        private readonly string updateStorageLocation;
        private readonly string updateStorageFileNamePattern;
        private readonly string errorUpdateStorageLocation;
        private readonly string errorUpdateStorageFileNamePattern;
        private readonly string currentDir;
        public FilesService(IConfiguration configuration, ILogger<FilesService> logger)
        {

            var workingDirectory = Directory.GetParent(Environment.CurrentDirectory)!;
#if Docker
            currentDir = workingDirectory.FullName;
#else
            currentDir = workingDirectory.Parent!.Parent!.FullName;
#endif

            this.logger = logger;
            updateStorageLocation = configuration.GetSection("UpdatesStorage").GetValue<string>("FolderName")!;
            updateStorageFileNamePattern = configuration.GetSection("UpdatesStorage").GetValue<string>("FileNamePattern")!;
            errorUpdateStorageLocation = configuration.GetSection("ErrorUpdatesStorage").GetValue<string>("FolderName")!;
            errorUpdateStorageFileNamePattern = configuration.GetSection("ErrorUpdatesStorage").GetValue<string>("FileNamePattern")!;
        }

        public void WriteObjectToJSON(string fullPath, object obj)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    using (var filestream = File.Create(fullPath))
                    {
                        var text = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        }));
                        filestream.Write(text);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred in WriteObjectToJSON method");
            }
        }

        public void WriteErrorUpdate(Update update)
        {
            var folderPath = Path.Combine(currentDir, errorUpdateStorageLocation);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var datetime = DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss.ffff");
            var fullPath = Path.Combine(folderPath, string.Format(errorUpdateStorageFileNamePattern, datetime));

            WriteObjectToJSON(fullPath, update);
        }

        public void WriteUpdate(Update update)
        {
            var folderPath = Path.Combine(currentDir, updateStorageLocation);
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var datetime = DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss.ffff");
            var fullPath = Path.Combine(folderPath, string.Format(updateStorageFileNamePattern, datetime));

            WriteObjectToJSON(fullPath, update);
        }
    }
}
