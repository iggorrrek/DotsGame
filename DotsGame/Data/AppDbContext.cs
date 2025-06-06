using Microsoft.EntityFrameworkCore;
using DotsGame.Models;

namespace DotsGame.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GameHistory> GameHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=DotsDB;Trusted_Connection=True;",
                options => options.EnableRetryOnFailure());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameHistory>(entity =>
            {
                entity.ToTable("GameHistories");
                entity.HasOne(gh => gh.User)
                      .WithMany(u => u.Games)
                      .HasForeignKey(gh => gh.UserId);
            });
        }
    }
}