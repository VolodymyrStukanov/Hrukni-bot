using ConsoleApp1.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1.DB
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Settings>(entity =>
            {
                entity.HasData(new Settings()
                {
                    Id = -1,
                    Token = "6376204287:AAELhYb3664qx-QWbyAUW8oK0psZuVhwT9c"
                });
            });

            builder.Entity<Member>(entity =>
            {
                entity.HasKey(x => new { x.Username, x.ChatId });

                entity.HasOne(x => x.Chat)
                .WithMany()
                .HasForeignKey(x => x.ChatId);
            });

            builder.Entity<Hohol>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => new { x.Username, x.ChatId });
            });

        }

        public DbSet<Settings> Settings { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Hohol> Hohols { get; set; }
        public DbSet<Member> Members { get; set; }

    }

}
