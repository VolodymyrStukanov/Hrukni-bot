using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.CommonService;
using HrukniHohlinaBot.Services.Interfaces;
using HrukniHohlinaBot.Services.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;

namespace HrukniNunitTest
{
    public class CommonServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> logger;

        public CommonServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "test_in_memory_db")
                .Options;
            _context = new ApplicationDbContext(options);

            var mock = new Mock<ILogger<UnitOfWork>>();
            logger = mock.Object;
            
            SetTestData();
        }

        private void SetTestData()
        {
            Chat chat = null;
            //chat = new Chat() { Id = 10l, IsActive = false };
            //_context.Chats.Add(chat);
            //_context.Entry(chat).State = EntityState.Detached;

            chat = new Chat() { Id = 20l, IsActive = false };
            _context.Chats.Add(chat);

            chat = new Chat() { Id = 13l, IsActive = false };
            _context.Chats.Add(chat);

            chat = new Chat() { Id = 11l, IsActive = false };
            _context.Chats.Add(chat);

            _context.SaveChanges();
            _context.Entry(chat).State = EntityState.Detached;


            var member = new Member() { ChatId = 11l, Id = 1l, IsOwner = false, Username = "member1" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 11l, Id = 2l, IsOwner = false, Username = "member2" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 11l, Id = 3l, IsOwner = false, Username = "member3" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 11l, Id = 30l, IsOwner = false, Username = "member30" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 11l, Id = 20l, IsOwner = false, Username = "member20" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 11l, Id = 4l, IsOwner = true, Username = "member4" };
            _context.Members.Add(member);
            member = new Member() { ChatId = 11l, Id = 5l, IsOwner = true, Username = "member5" };
            _context.Members.Add(member);
            _context.SaveChanges();

            Hohol hohol = null;
            //hohol = new Hohol() { ChatId = 11l, Chat = chat, MemberId = 2l, Member = memeber, AssignmentDate = DateTime.Now.AddHours(-12), EndWritingPeriod = DateTime.Now.AddMinutes(10) };
            //_context.Hohols.Add(hohol);
            //hohol = new Hohol() { ChatId = 11l, Chat = chat, MemberId = 3l, Member = memeber, AssignmentDate = DateTime.Now.AddHours(-12), EndWritingPeriod = DateTime.Now.AddMinutes(-10) };
            //_context.Hohols.Add(hohol);
            //hohol = new Hohol() { ChatId = 11l, Chat = chat, MemberId = 4l, Member = memeber, AssignmentDate = DateTime.Now.AddDays(-2), EndWritingPeriod = DateTime.Now.AddDays(-2) };
            hohol = new Hohol()
            {
                ChatId = 20l,
                MemberId = 30l,
                AssignmentDate = DateTime.UtcNow,
                EndWritingPeriod = DateTime.UtcNow,
            };
            _context.Hohols.Add(hohol);
            _context.SaveChanges();
        }

        #region ChatTest

        [Test, Order(1)]
        public void AddChat()
        {
            //Arrange
            var chatService = new CommonService<Chat>(_context);
            var chat = new Chat()
            {
                Id = 1l,
                IsActive = false,
            };
            var unitOfWork = new UnitOfWork(this._context, this.logger);

            //Act
            var createdChat = chatService.Add(chat);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.NotNull(createdChat);
        }

        [Test, Order(2)]
        public void AddExistingChat()
        {
            //Arrange
            var chatService = new CommonService<Chat>(_context);
            var chat = new Chat()
            {
                Id = 1l,
                IsActive = false,
            };
            var unitOfWork = new UnitOfWork(this._context, this.logger);

            //Act
            var del = () =>
            {
                chatService.Add(chat);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<ArgumentException>(td);
        }

        [Test, Order(3)]
        public void GetExistingChatById()
        {
            //Arrange
            var chatService = new CommonService<Chat>(_context);
            var chatId = 1l;

            //Act
            var chat = chatService.Get(key: chatId);

            //Assert
            ClassicAssert.NotNull(chat);
        }

        [Test, Order(4)]
        public void GetNotExistingChatById()
        {
            //Arrange
            var chatService = new CommonService<Chat>(_context);
            var chatId = 100l;

            //Act
            var chat = chatService.Get(key: chatId);

            //Assert
            ClassicAssert.Null(chat);
        }

        [Test, Order(5)]
        public void UpdateExistingChat()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatService = new CommonService<Chat>(_context);
            var chatId = 1l;
            var isActive = true;
            var chat = chatService.Get(chatId);
            _context.Entry(chat).State = EntityState.Detached;

            //Act
            chatService.Update(new Chat() { Id = chatId, IsActive = isActive });
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(chatService.Get(chatId).Id, chatId);
            ClassicAssert.AreEqual(chatService.Get(chatId).IsActive, isActive);
        }

        [Test, Order(6)]
        public void UpdateNotExistingChat()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatService = new CommonService<Chat>(_context);
            var chatId = 2l;
            var isActive = true;

            //Act
            var del = () =>
            {
                chatService.Update(new Chat() { Id = chatId, IsActive = isActive });
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<DbUpdateConcurrencyException>(td);
        }

        [Test, Order(7)]
        public void DeleteExistingChatById()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatService = new CommonService<Chat>(_context);
            var chatId = 10l;
            chatService.Add(new Chat() { Id = 10l, IsActive = false });
            unitOfWork.SaveChanges();

            //Act
            chatService.Remove(chatId);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(chatService.Get(chatId), null);
        }

        [Test, Order(8)]
        public void DeleteNotExistingChatById()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatService = new CommonService<Chat>(_context);
            var chatId = 100l;

            //Act
            var del = () =>
            {
                chatService.Remove(chatId);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(9)]
        public void DeleteExistingChat()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatService = new CommonService<Chat>(_context);
            var chat = new Chat() { Id = 13l, IsActive = true };

            //Act
            chatService.Remove(chat);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(chatService.Get(chat.Id), null);
        }

        [Test, Order(10)]
        public void DeleteNotExistingChat()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatService = new CommonService<Chat>(_context);
            var chat = new Chat() { Id = 130l, IsActive = true };

            //Act
            var del = () =>
            {
                chatService.Remove(chat);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<DbUpdateConcurrencyException>(td);
        }

        #endregion ChatTest

        #region MemberTest

        [Test, Order(11)]
        public void AddMember()
        {
            //Arrange
            var memberService = new CommonService<Member>(_context);
            var member = new Member()
            {
                ChatId = 11l,
                Id = 10l,
                IsOwner = true,
                Username = "member10"
            };
            var unitOfWork = new UnitOfWork(this._context, this.logger);

            //Act
            var createdMember = memberService.Add(member);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.NotNull(createdMember);
        }

        [Test, Order(12)]
        public void AddExistingMember()
        {
            //Arrange
            var memberService = new CommonService<Member>(_context);
            var member = new Member()
            {
                ChatId = 11l,
                Id = 10l,
                IsOwner = true,
                Username = "member10"
            };
            var unitOfWork = new UnitOfWork(this._context, this.logger);

            //Act
            var del = () =>
            {
                memberService.Add(member);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<ArgumentException>(td);
        }


        [Test, Order(13)]
        public void GetExistingMemberByKey()
        {
            //Arrange
            var memberService = new CommonService<Member>(_context);
            var memberId = 1l;
            var chatId = 11l;

            //Act
            var member = memberService.Get(memberId, chatId);

            //Assert
            ClassicAssert.NotNull(member);
        }

        [Test, Order(14)]
        public void GetNotExistingMemberByKey()
        {
            //Arrange
            var memberService = new CommonService<Member>(_context);
            var memberId = 100l;
            var chatId = 11l;

            //Act
            var member = memberService.Get(memberId, chatId);

            //Assert
            ClassicAssert.Null(member);
        }

        [Test, Order(15)]
        public void GetExistingMemberIncludingChildsByKey()
        {
            //Arrange
            var memberService = new CommonService<Member>(_context);
            var memberId = 1l;
            var chatId = 11l;

            //Act
            var member = memberService.GetIncludingChildren(memberId, chatId);

            //Assert
            ClassicAssert.NotNull(member);
            ClassicAssert.NotNull(member.Chat);
        }


        [Test, Order(16)]
        public void UpdateExistingMember()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var memberId = 1l;
            var member = memberService.Get(memberId, chatId);
            _context.Entry(member).State = EntityState.Detached;

            var updatedMember = new Member()
            {
                ChatId = chatId,
                Id = memberId,
                IsOwner = !member.IsOwner,
                Username = "updatedUsername"
            };

            //Act
            memberService.Update(updatedMember);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(memberService.Get(memberId, chatId).IsOwner, updatedMember.IsOwner);
            ClassicAssert.AreEqual(memberService.Get(memberId, chatId).Username, updatedMember.Username);
        }

        [Test, Order(17)]
        public void UpdateNotExistingMember()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var memberId = 100l;

            var updatedMember = new Member()
            {
                ChatId = chatId,
                Id = memberId,
                IsOwner = false,
                Username = "updatedUsername"
            };

            //Act
            var del = () =>
            {
                memberService.Update(updatedMember);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<DbUpdateConcurrencyException>(td);
        }

        [Test, Order(18)]
        public void DeleteExistingMemberByKey()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var memberId = 1l;

            //Act
            memberService.Remove(memberId, chatId);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(memberService.Get(memberId, chatId), null);
        }

        [Test, Order(19)]
        public void DeleteNotExistingMemberByKey()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var memberId = 1l;

            //Act
            var del = () =>
            {
                memberService.Remove(memberId, chatId);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(20)]
        public void DeleteExistingMember()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var memberId = 20l;

            var member = memberService.Get(memberId, chatId);

            //Act
            memberService.Remove(member);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(memberService.Get(memberId, chatId), null);
        }

        [Test, Order(21)]
        public void DeleteNotExistingMember()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var memberId = 1l;

            var member = new Member()
            {
                ChatId = chatId,
                Id = memberId,
                IsOwner = false,
                Username = "username"
            };

            //Act
            var del = () =>
            {
                memberService.Remove(member);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<DbUpdateConcurrencyException>(td);
        }

        #endregion MemberTest

        #region HoholTest

        [Test, Order(30)]
        public void AddHohol()
        {
            //Arrange
            var hoholService = new CommonService<Hohol>(_context);
            var chatService = new CommonService<Chat>(_context);
            var memberService = new CommonService<Member>(_context);
            var chatId = 11l;
            var MemberId = 5l;

            var hohol = new Hohol()
            {
                ChatId = chatId,
                MemberId = 5l,
                Chat = chatService.Get(chatId),
                Member = memberService.Get(MemberId, chatId)
            };
            var unitOfWork = new UnitOfWork(this._context, this.logger);

            //Act
            var createdHohol = hoholService.Add(hohol);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.NotNull(createdHohol);
            ClassicAssert.NotNull(hoholService.Get(hohol.ChatId));
        }

        [Test, Order(31)]
        public void AddExistingHohol()
        {
            //Arrange
            var hoholService = new CommonService<Hohol>(_context);
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatId = 11l;
            var hohol = hoholService.Get(chatId);
            _context.Entry(hohol).State = EntityState.Detached;


            //Act
            var del = () =>
            {
                hoholService.Add(hohol);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<ArgumentException>(td);
        }


        [Test, Order(32)]
        public void GetExistingHoholByChatId()
        {
            //Arrange
            var hoholService = new CommonService<Hohol>(_context);
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatId = 11l;

            //Act
            var hohol = hoholService.Get(chatId);

            //Assert
            ClassicAssert.NotNull(hohol);
        }

        [Test, Order(33)]
        public void GetNotExistingHoholByChatId()
        {
            //Arrange
            var hoholService = new CommonService<Hohol>(_context);
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var chatId = 12l;

            //Act
            var hohol = hoholService.Get(chatId);

            //Assert
            ClassicAssert.Null(hohol);
        }

        [Test, Order(34)]
        public void GetExistingHoholIncludingChildsByChatId()
        {
            //Arrange
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 11l;

            //Act
            var hohol = hoholService.GetIncludingChildren(chatId);

            //Assert
            ClassicAssert.NotNull(hohol);
            ClassicAssert.NotNull(hohol.Chat);
            ClassicAssert.NotNull(hohol.Member);
        }


        [Test, Order(35)]
        public void UpdateExistingHohol()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 11l;
            var hohol = hoholService.Get(chatId);
            _context.Entry(hohol).State = EntityState.Detached;

            var updatedHohol = new Hohol()
            {
                ChatId = chatId,
                MemberId = hohol.MemberId,
                AssignmentDate = DateTime.UtcNow,
                EndWritingPeriod = DateTime.UtcNow,
            };

            //Act
            hoholService.Update(updatedHohol);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(hoholService.Get(chatId).AssignmentDate, updatedHohol.AssignmentDate);
            ClassicAssert.AreEqual(hoholService.Get(chatId).EndWritingPeriod, updatedHohol.EndWritingPeriod);
        }

        [Test, Order(36)]
        public void UpdateNotExistingHohol()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 12l;
            var memberId = 12l;

            var updatedHohol = new Hohol()
            {
                ChatId = chatId,
                MemberId = memberId,
                AssignmentDate = DateTime.UtcNow,
                EndWritingPeriod = DateTime.UtcNow,
            };

            //Act
            var del = () =>
            {
                hoholService.Update(updatedHohol);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<DbUpdateConcurrencyException>(td);
        }

        [Test, Order(37)]
        public void DeleteExistingHoholByKey()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 11l;

            //Act
            hoholService.Remove(chatId);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(hoholService.Get(chatId), null);
        }

        [Test, Order(38)]
        public void DeleteNotExistingHoholByKey()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 12l;

            //Act
            var del = () =>
            {
                hoholService.Remove(chatId);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.DoesNotThrow(td);
        }

        [Test, Order(39)]
        public void DeleteExistingHohol()
        {
            //Arranges
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 20l;
            var hohol = hoholService.Get(chatId);

            //Act
            hoholService.Remove(hohol);
            unitOfWork.SaveChanges();

            //Assert
            ClassicAssert.AreEqual(hoholService.Get(chatId), null);
        }

        [Test, Order(40)]
        public void DeleteNotExistingHohol()
        {
            //Arrange
            var unitOfWork = new UnitOfWork(this._context, this.logger);
            var hoholService = new CommonService<Hohol>(_context);
            var chatId = 20l;
            var memberId = 20l;

            var updatedHohol = new Hohol()
            {
                ChatId = chatId,
                MemberId = memberId,
                AssignmentDate = DateTime.UtcNow,
                EndWritingPeriod = DateTime.UtcNow,
            };

            //Act
            var del = () =>
            {
                hoholService.Remove(updatedHohol);
                unitOfWork.SaveChanges();
            };
            TestDelegate td = new TestDelegate(del);

            //Assert
            ClassicAssert.Throws<DbUpdateConcurrencyException>(td);
        }

        #endregion HoholTest
    }
}