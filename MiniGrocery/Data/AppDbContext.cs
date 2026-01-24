using Microsoft.EntityFrameworkCore;
using MiniGrocery.Models;

namespace MiniGrocery.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Rice",
                    Price = 50,
                    Stock = 100
                },
                new Product
                {
                    Id = 2,
                    Name = "Milk",
                    Price = 30,
                    Stock = 50
                },
                new Product
                {
                    Id = 3,
                    Name = "Bread",
                    Price = 25,
                    Stock = 40
                }
            );
        }
    }
}
