using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.DB;
using Microsoft.EntityFrameworkCore;
using HrukniHohlinaBot.Services.CommonServices;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.UnitOfWork;
using Telegram.Bot.Types;
using HrukniHohlinaBot.Services.BotServices;
using HrukniNunitTest.ServicesForTesting;
using Newtonsoft.Json;
using NUnit.Framework.Legacy;
using Microsoft.Extensions.Logging;
using Moq;

namespace HrukniNunitTest
{
    public class UpdateHandlerServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> logger;
        private readonly UpdateHandlerService _updateHandlerService;

        public UpdateHandlerServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "test_in_memory_db")
                .Options;
            _context = new ApplicationDbContext(options);

            var mock = new Mock<ILogger<UnitOfWork>>();
            logger = mock.Object;

            var unitOfWork = new UnitOfWork(_context, logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatService = new CommonService<HrukniHohlinaBot.DB.Models.Chat>(_context);
            var memberService = new CommonService<Member>(_context);
            var resetHoholService = new HoholsService(null, unitOfWork, chatService, hoholService, memberService);

            var updateHandlerService = new UpdateHandlerService(null,null, resetHoholService, unitOfWork, null, null, memberService, chatService, hoholService);

            _updateHandlerService = updateHandlerService;
        }

        [Test, Order(1)]
        public async Task AddChat()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_bot_to_chat_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_bot_to_chat_2.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Update up2 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file2), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            await _updateHandlerService.HandleUpdate(up1);
            await _updateHandlerService.HandleUpdate(up2);

            //Assert
            ClassicAssert.AreNotEqual(_context.Chats.ToArray().Length, 0);
        }

        [Test, Order(2)]
        public async Task MakeBotAdmin()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Bot_admin.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            Func<Task> del = async () =>
            {
                await _updateHandlerService.HandleUpdate(up1);
            };

            TestDelegate td = new TestDelegate(() => del.Invoke());

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(3)]
        public async Task AddMember()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_member_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Add_member_2.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Update up2 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file2), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            await _updateHandlerService.HandleUpdate(up1);
            await _updateHandlerService.HandleUpdate(up2);

            //Assert
            ClassicAssert.AreNotEqual(_context.Members.ToArray().Length, 0);
        }

        [Test, Order(4)]
        public async Task MakeChatActive()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Admin_start_hrukni.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            await _updateHandlerService.HandleUpdate(up1);

            //Assert
            ClassicAssert.AreEqual(_context.Chats.Single().IsActive, true);
        }

        [Test, Order(5)]
        public async Task ResetHohols()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Admin_reset_hohols.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            await _updateHandlerService.HandleUpdate(up1);

            //Assert
            ClassicAssert.AreNotEqual(_context.Hohols.ToArray().Length, 0);
        }

        [Test, Order(6)]
        public async Task SendingNotAllowedMessage()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Member_message.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            up1.Message.Date = DateTime.Now.ToUniversalTime();

            //Act
            Func<Task> del = async () =>
            {
                await _updateHandlerService.HandleUpdate(up1);
            };
            TestDelegate td = new TestDelegate(() => del.Invoke());

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(7)]
        public async Task SendingHruMessage()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Member_hru.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            up1.Message.Date = DateTime.Now.ToUniversalTime();

            //Act
            await _updateHandlerService.HandleUpdate(up1);

            //Assert
            ClassicAssert.AreEqual(_context.Hohols.Single().IsAllowedToWrite(), true);
        }

        [Test, Order(8)]
        public async Task RemovingMember()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_member_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_member_2.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Update up2 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file2), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            await _updateHandlerService.HandleUpdate(up1);
            await _updateHandlerService.HandleUpdate(up2);

            //Assert
            ClassicAssert.AreEqual(_context.Members.ToArray().Length, 1);
        }

        [Test, Order(9)]
        public async Task RemovingBotFromChat()
        {
            //Arrange
            string file1 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_bot_1.json";
            string file2 = "D:\\study\\preparation_for_interview\\ASP.NET\\telegram_bots\\Update_JSON\\Remove_bot_2.json";
            Update up1 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file1), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Update up2 = JsonConvert.DeserializeObject<Update>(TestFilesService.LoadFile(file2), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            //Act
            await _updateHandlerService.HandleUpdate(up1);
            await _updateHandlerService.HandleUpdate(up2);

            //Assert
            ClassicAssert.AreEqual(_context.Chats.ToArray().Length, 0);
        }

    }
}
