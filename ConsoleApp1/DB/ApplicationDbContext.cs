using HrukniHohlinaBot.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.DB
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
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
                entity.HasKey(x => new { x.Id, x.ChatId });

                entity.HasOne(x => x.Chat)
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
            });

            builder.Entity<Hohol>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => new { x.MemberId, x.ChatId })
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
            });
        }

        public DbSet<Settings> Settings { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Hohol> Hohols { get; set; }
        public DbSet<Member> Members { get; set; }

    }

}
