using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.DB;
using Microsoft.EntityFrameworkCore;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.UnitOfWork;
using NUnit.Framework.Legacy;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using HrukniBot.Services.ChatServices;
using HrukniBot.Services.MemberServices;

namespace HrukniNunitTest
{
    public class ResetHoholServiceTest
    {
        private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;

        public ResetHoholServiceTest()
        {
            dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "test_in_memory_db")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        [SetUp]
        public void DataSeeding()
        {
            var context = new ApplicationDbContext(dbContextOptions);

            Chat chat = null;
            chat = new Chat() { Id = 1l, IsActive = false };
            context.Chats.Add(chat);
            chat = new Chat() { Id = 2l, IsActive = false };
            context.Chats.Add(chat);
            context.SaveChanges();

            Member member = null;
            member = new Member() { ChatId = 1l, Id = 1l, IsOwner = false, Username = "member1" };
            context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 2l, IsOwner = true, Username = "member2" };
            context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 3l, IsOwner = false, Username = "member3" };
            context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 4l, IsOwner = false, Username = "member4" };
            context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 5l, IsOwner = false, Username = "member5" };
            context.Members.Add(member);
            member = new Member() { ChatId = 2l, Id = 2l, IsOwner = false, Username = "member2" };
            context.Members.Add(member);
            member = new Member() { ChatId = 2l, Id = 3l, IsOwner = false, Username = "member3" };
            context.Members.Add(member);
            member = new Member() { ChatId = 2l, Id = 6l, IsOwner = false, Username = "member6" };
            context.Members.Add(member);
            context.SaveChanges();

            Hohol hohol = null;
            hohol = new Hohol() { ChatId = 1l, MemberId = 2l, AssignmentDate = DateTime.Now.AddHours(-12), EndWritingPeriod = DateTime.Now.AddMinutes(10) };
            context.Hohols.Add(hohol);
            context.SaveChanges();
        }

        [Test, Order(1)]
        public void ResetHoholsForAllChats()
        {
            //Arrange
            var mockHS = new Mock<ILogger<HoholsService>>();
            var loggerHS = mockHS.Object;
            var mockUoW = new Mock<ILogger<UnitOfWork>>();
            var loggerUoW = mockUoW.Object;
            var context = new ApplicationDbContext(dbContextOptions);
            var chatService = new ChatService(context);
            var memberService = new MemberService(context);
            var unitOfWork = new UnitOfWork(context, loggerUoW);
            var hoholService = new HoholsService(context, loggerHS, unitOfWork, chatService, memberService);

            var chat1Id = 1l;
            var chat2Id = 2l;

            //Act
            hoholService.ResetHohols();

            //Assert
            var hoholForChat1 = hoholService.GetHohol(chat1Id);
            var hoholForChat2 = hoholService.GetHohol(chat2Id);
            ClassicAssert.NotNull(hoholForChat1);
            ClassicAssert.IsTrue(hoholForChat1.IsActive());
            ClassicAssert.IsFalse(hoholForChat1.IsAllowedToWrite());
            ClassicAssert.NotNull(hoholForChat2);
            ClassicAssert.IsTrue(hoholForChat2.IsActive());
            ClassicAssert.IsFalse(hoholForChat2.IsAllowedToWrite());
        }
    }
}
