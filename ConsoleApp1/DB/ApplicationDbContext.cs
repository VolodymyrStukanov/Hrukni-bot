﻿using HrukniHohlinaBot.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.DB
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(x => new { x.Id, x.ChatId });

                entity.HasOne(x => x.Chat)
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Hohol>(entity =>
            {
                entity.HasKey(x => x.ChatId);

                entity.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => new { x.MemberId, x.ChatId })
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

                entity.HasOne(x => x.Chat)
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Hohol> Hohols { get; set; }
        public DbSet<Member> Members { get; set; }

    }

}
