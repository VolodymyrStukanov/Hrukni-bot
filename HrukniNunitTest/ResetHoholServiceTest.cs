using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.DB;
using Microsoft.EntityFrameworkCore;
using HrukniHohlinaBot.Services.ResetHoholServices;
using HrukniHohlinaBot.Services.CommonServices;
using HrukniHohlinaBot.Services.UnitOfWork;
using NUnit.Framework.Legacy;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HrukniNunitTest
{
    public class ResetHoholServiceTest
    {
        private readonly ApplicationDbContext _context;

        public ResetHoholServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "test_in_memory_db")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new ApplicationDbContext(options);

            SetTestData();
        }

        private void SetTestData()
        {
            Chat chat = null;
            chat = new Chat() { Id = 1l, IsActive = false };
            _context.Chats.Add(chat);
            chat = new Chat() { Id = 2l, IsActive = false };
            _context.Chats.Add(chat);
            _context.SaveChanges();

            Member member = null;
            member = new Member() { ChatId = 1l, Id = 1l, IsOwner = false, Username = "member1" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 2l, IsOwner = true, Username = "member2" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 3l, IsOwner = false, Username = "member3" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 4l, IsOwner = false, Username = "member4" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 1l, Id = 5l, IsOwner = false, Username = "member5" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 2l, Id = 2l, IsOwner = false, Username = "member2" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 2l, Id = 3l, IsOwner = false, Username = "member3" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 2l, Id = 6l, IsOwner = false, Username = "member6" };
            _context.Members.Add(member);
            _context.SaveChanges();

            Hohol hohol = null;
            hohol = new Hohol() { ChatId = 1l, MemberId = 2l, AssignmentDate = DateTime.Now.AddHours(-12), EndWritingPeriod = DateTime.Now.AddMinutes(10) };
            _context.Hohols.Add(hohol);
            _context.SaveChanges();
        }

        [Test, Order(1)]
        public void ResetHoholsForAllChats()
        {
            //Arrange
            var chatService = new CommonService<Chat>(_context);
            var memberService = new CommonService<Member>(_context);
            var hoholService = new CommonService<Hohol>(_context);
            var unitOfWork = new UnitOfWork(_context);
            var resetHoholServie = new ResetHoholService(null, unitOfWork, chatService, hoholService, memberService);

            var chat1Id = 1l;
            var chat2Id = 2l;

            //Act
            resetHoholServie.ResetHohols();

            //Assert
            var hoholForChat1 = hoholService.Get(chat1Id);
            var hoholForChat2 = hoholService.Get(chat2Id);
            ClassicAssert.NotNull(hoholForChat1);
            ClassicAssert.IsTrue(hoholForChat1.IsActive());
            ClassicAssert.IsFalse(hoholForChat1.IsAllowedToWrite());
            ClassicAssert.NotNull(hoholForChat2);
            ClassicAssert.IsTrue(hoholForChat2.IsActive());
            ClassicAssert.IsFalse(hoholForChat2.IsAllowedToWrite());
        }
    }
}
