using Microsoft.EntityFrameworkCore;
using MyUser.API.Models;

namespace MyUser.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }



        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
        .Property(p => p.Price)
        .HasColumnType("decimal(18, 2)");
           

            base.OnModelCreating(modelBuilder);
        }
    }
}