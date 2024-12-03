using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.DB;
using Microsoft.EntityFrameworkCore;
using HrukniHohlinaBot.Services.CommonService;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.UnitOfWork;
using Telegram.Bot.Types;
using HrukniHohlinaBot.Services.BotServices;
using HrukniNunitTest.ServicesForTesting;
using Newtonsoft.Json;
using NUnit.Framework.Legacy;
using Microsoft.Extensions.Logging;
using Moq;
using HrukniBot.Services.ChatServices;
using HrukniBot.Services.MemberServices;

namespace HrukniNunitTest
{
    public class UpdateHandlerServiceTest
    {
        private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;

        public UpdateHandlerServiceTest()
        {
            dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "test_in_memory_db")
                .Options;
        }

        private ApplicationDbContext GetContext()
        {
            return new ApplicationDbContext(dbContextOptions);
        }

        private UpdateHandlerService GetUpdateHandlerService()
        {
            var mockUoW = new Mock<ILogger<UnitOfWork>>();
            var loggerUoW = mockUoW.Object;
            var mockHS = new Mock<ILogger<HoholsService>>();
            var loggerHS = mockHS.Object;
            var mockUHS = new Mock<ILogger<UpdateHandlerService>>();
            var loggerUHS = mockUHS.Object;

            var context = GetContext();
            var unitOfWork = new UnitOfWork(context, loggerUoW);
            var chatService = new ChatService(context);
            var memberService = new MemberService(context);
            var hoholService = new HoholsService(context, loggerHS, unitOfWork, chatService, memberService);
            return new UpdateHandlerService(loggerUHS, null, unitOfWork, null, null, memberService, chatService, hoholService);
        }

        private Update ReadUpdateFromJSON(string path)
        {
            var update = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(path), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (update == null) throw new Exception("Test file are not found");

            return update;
        }

        [Test, Order(1)]
        public async Task AddChat()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();

            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_bot_to_chat_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_bot_to_chat_2.json";
            Update update1 = ReadUpdateFromJSON(file1);
            Update update2 = ReadUpdateFromJSON(file2);

            //Act
            await updateHandlerService.HandleUpdate(update1);
            await updateHandlerService.HandleUpdate(update2);

            //Assert
            Assert.That(await context.Chats?.ToArrayAsync()!, Has.Length.EqualTo(1));
        }

        [Test, Order(2)]
        public void MakeBotAdmin()
        {
            //Arrange
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Bot_admin.json";
            Update update = ReadUpdateFromJSON(file1);

            //Act
            Func<Task> del = async () =>
            {
                await updateHandlerService.HandleUpdate(update);
            };

            TestDelegate td = new TestDelegate(() => del.Invoke());

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(3)]
        public async Task AddMember()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_member_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_member_2.json";
            Update update1 = ReadUpdateFromJSON(file1);
            Update update2 = ReadUpdateFromJSON(file2);

            //Act
            await updateHandlerService.HandleUpdate(update1);
            await updateHandlerService.HandleUpdate(update2);

            //Assert
            Assert.That(await context.Members?.ToArrayAsync()!, Has.Length.EqualTo(2));
        }

        [Test, Order(4)]
        public async Task MakeChatActive()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Admin_start_hrukni.json";
            Update update1 = ReadUpdateFromJSON(file1);

            //Act
            await updateHandlerService.HandleUpdate(update1);

            //Assert
            Assert.That((await context.Chats?.SingleAsync()!).IsActive, Is.EqualTo(true));
        }

        [Test, Order(5)]
        public async Task ResetHohols()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Admin_reset_hohols.json";
            Update update1 = ReadUpdateFromJSON(file1);

            //Act
            await updateHandlerService.HandleUpdate(update1);

            //Assert
            Assert.That(await context.Hohols?.ToArrayAsync()!, Has.Length.EqualTo(1));
        }

        [Test, Order(6)]
        public void SendingNotAllowedMessage()
        {
            //Arrange
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Member_message.json";
            Update update1 = ReadUpdateFromJSON(file1);
            update1.Message!.Date = DateTime.Now.ToUniversalTime();

            //Act
            Func<Task> del = async () =>
            {
                await updateHandlerService.HandleUpdate(update1);
            };
            TestDelegate td = new TestDelegate(() => del.Invoke());

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(7)]
        public async Task SendingHruMessage()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Member_hru.json";
            Update update1 = ReadUpdateFromJSON(file1);
            update1.Message!.Date = DateTime.Now.ToUniversalTime();

            //Act
            await updateHandlerService.HandleUpdate(update1);

            //Assert
            Assert.That((await context.Hohols?.SingleAsync()!).IsAllowedToWrite(), Is.EqualTo(true));
        }

        [Test, Order(8)]
        public async Task RemovingMember()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_member_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_member_2.json";
            Update update1 = ReadUpdateFromJSON(file1);
            Update update2 = ReadUpdateFromJSON(file2);

            //Act
            await updateHandlerService.HandleUpdate(update1);
            await updateHandlerService.HandleUpdate(update2);

            //Assert
            Assert.That(await context.Members?.ToArrayAsync()!, Has.Length.EqualTo(1));
        }

        [Test, Order(9)]
        public async Task RemovingBotFromChat()
        {
            //Arrange
            var context = GetContext();
            var updateHandlerService = GetUpdateHandlerService();
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_bot_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_bot_2.json";
            Update update1 = ReadUpdateFromJSON(file1);
            Update update2 = ReadUpdateFromJSON(file2);

            //Act
            await updateHandlerService.HandleUpdate(update1);
            await updateHandlerService.HandleUpdate(update2);

            //Assert
            Assert.That(await context.Chats?.ToArrayAsync()!, Has.Length.EqualTo(0));
        }

    }
}
