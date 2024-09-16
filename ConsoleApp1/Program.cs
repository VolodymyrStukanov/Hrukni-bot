using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.BotServices;
using HrukniHohlinaBot.Services.CommonServices;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;


namespace HrukniHohlinaBot
{
    static class Program
    {
        static async Task Main()
        {

            var builder = new ConfigurationBuilder(); 
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();

            IConfiguration config = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/error/log-.txt", rollingInterval: RollingInterval.Day/*, restrictedToMinimumLevel: LogEventLevel.Warning*/)
                .CreateLogger();

            using var cts = new CancellationTokenSource();
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(
                        token: config.GetSection("BotSettings").GetValue<string>("Token")
                        ));
                    services.AddTransient<TelegramBotService>();

                    services.AddTransient<UnitOfWork>();
                    services.AddTransient<HoholService>();
                    services.AddTransient<CommonService<Chat>>();
                    services.AddTransient<CommonService<Member>>();

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
                    });
                })
                .UseSerilog()
                .Build();

            host.RunAsync();

            var botService = host.Services.GetRequiredService<TelegramBotService>();
            botService.StartBotAsync(cts.Token);
        }
    }
}