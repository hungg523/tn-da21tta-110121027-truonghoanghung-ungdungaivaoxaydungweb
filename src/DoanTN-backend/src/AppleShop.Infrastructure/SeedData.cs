using AppleShop.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace AppleShop.Infrastructure
{
    public static class SeedData
    {
        public static void SeedDataGenerate(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "hungg",
                Email = "hoanghung52304@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456789Aa@!"),
                Role = 1,
                CreatedAt = DateTime.Now,
                IsActived = 1
            });
        }
    }
}