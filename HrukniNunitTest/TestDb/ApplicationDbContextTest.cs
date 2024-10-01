using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HrukniNunitTest.TestDb
{
    public class ApplicationDbContextTest : ApplicationDbContext
    {
        public ApplicationDbContextTest(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Chat>(entity =>
            {
                entity.HasData(new Chat() { Id = 1l, IsActive = false });
                entity.HasData(new Chat() { Id = 2l, IsActive = true });
            });


            builder.Entity<Member>(entity =>
            {
                entity.HasData(new Member()
                {
                    ChatId = 1l,
                    Id = 1l,
                    IsOwner = true,
                    Username = "member1"
                });
                entity.HasData(new Member()
                {
                    ChatId = 1l,
                    Id = 2l,
                    IsOwner = false,
                    Username = "member2"
                });
                entity.HasData(new Member()
                {
                    ChatId = 1l,
                    Id = 3l,
                    IsOwner = false,
                    Username = "member3"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 1l,
                    IsOwner = false,
                    Username = "member1"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 2l,
                    IsOwner = false,
                    Username = "member2"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 4l,
                    IsOwner = false,
                    Username = "member4"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 5l,
                    IsOwner = false,
                    Username = "member5"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 6l,
                    IsOwner = false,
                    Username = "member6"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 7l,
                    IsOwner = false,
                    Username = "member7"
                });
                entity.HasData(new Member()
                {
                    ChatId = 2l,
                    Id = 8l,
                    IsOwner = true,
                    Username = "member8"
                });
            });
        }
    }
}
