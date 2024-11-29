using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.BotServices;
using HrukniHohlinaBot.Services.CommonService;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using HrukniHohlinaBot.Services.FilesService;
using HrukniBot.Services.ChatServices;
using HrukniBot.Services.MemberServices;
using HrukniBot.Services.ResetBackgroundService;
using HrukniBot.Services.HoholServices;
using HrukniBot.Services.FilesService;
using HrukniBot.Services.UnitOfWork;
using HrukniBot.Services.BotServices;


namespace HrukniHohlinaBot
{
    static class Program
    {
        static async Task Main()
        {

            var builder = new ConfigurationBuilder();

            IConfiguration config = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/error/log-.txt", rollingInterval: RollingInterval.Day/*, restrictedToMinimumLevel: LogEventLevel.Warning*/)
            .CreateLogger();

            using var cts = new CancellationTokenSource();
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) => {
                    config.SetBasePath(Directory.GetCurrentDirectory())
#if Docker
                    .AddJsonFile("appsettings.docker.json", optional: false)
#else
                    .AddJsonFile("appsettings.json", optional: false)
#endif
                    .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var registeredConfig = context.Configuration;

                    services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(
                        token: registeredConfig.GetSection("BotSettings").GetValue<string>("Token")!
                        ));
                    services.AddSingleton<TelegramBotService>();
                    services.AddSingleton<IFilesService, FilesService>();

                    services.AddTransient<IUnitOfWork, UnitOfWork>();
                    services.AddTransient<IUpdateHandlerService, UpdateHandlerService>();
                    services.AddTransient<IChatService, ChatService>();
                    services.AddTransient<IMemberService, MemberService>();
                    services.AddTransient<IHoholsService, HoholsService>();

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(registeredConfig.GetConnectionString("DefaultConnection"));
                    });

                    services.AddHostedService<ResetHoholsService>();
                })
                .UseSerilog()
                .Build();

            host.RunAsync();

            var botService = host.Services.GetRequiredService<TelegramBotService>();
            await botService.StartBotAsync(cts.Token);
        }
    }
}