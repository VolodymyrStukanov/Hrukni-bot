using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace HrukniNunitTest.TestDb
{
    public class ApplicationDbContextTest : ApplicationDbContext
    {
        public ApplicationDbContextTest(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasData(new Chat() { Id = 1L, IsActive = false });
                entity.HasData(new Chat() { Id = 2L, IsActive = true });
            });


            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasData(new Member()
                {
                    ChatId = 1L,
                    Id = 1L,
                    IsOwner = true,
                    Username = "member1"
                });
                entity.HasData(new Member()
                {
                    ChatId = 1L,
                    Id = 2L,
                    IsOwner = false,
                    Username = "member2"
                });
                entity.HasData(new Member()
                {
                    ChatId = 1L,
                    Id = 3L,
                    IsOwner = false,
                    Username = "member3"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 1L,
                    IsOwner = false,
                    Username = "member1"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 2L,
                    IsOwner = false,
                    Username = "member2"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 4L,
                    IsOwner = false,
                    Username = "member4"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 5L,
                    IsOwner = false,
                    Username = "member5"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 6L,
                    IsOwner = false,
                    Username = "member6"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 7L,
                    IsOwner = false,
                    Username = "member7"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2L,
                    Id = 8L,
                    IsOwner = true,
                    Username = "member8"
                });
            });
        }
    }
}
