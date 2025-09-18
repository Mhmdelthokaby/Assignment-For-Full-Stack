using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data
{
    public static class SeedData
    {
        public static async Task Seed(AppDbContext context)
        {
            // If we already have users, skip seeding
            if (context.Users.Any())
                return;

            var user1 = new User
            {
                Id = Guid.NewGuid(),
                UserName = "user1",
                NormalizedUserName = "USER1",
                Email = "user1@test.com",
                NormalizedEmail = "USER1@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, "Pass123$")
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                UserName = "user2",
                NormalizedUserName = "USER2",
                Email = "user2@test.com",
                NormalizedEmail = "USER2@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, "Pass123$")
            };

            var user3 = new User
            {
                Id = Guid.NewGuid(),
                UserName = "user3",
                NormalizedUserName = "USER3",
                Email = "user3@test.com",
                NormalizedEmail = "USER3@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, "Pass123$")
            };

            context.Users.AddRange(user1, user2, user3);

            var random = new Random();
            var users = new[] { user1, user2, user3 };
            int productId = 1;

            foreach (var u in users)
            {
                int count = random.Next(4, 11); // 4–10 products per user
                for (int i = 0; i < count; i++)
                {
                    int imgNum = random.Next(1, 5); // random number between 1 and 4

                    context.Products.Add(new Product
                    {
                        Name = $"Product {productId}",
                        ProductCode = $"P{productId:000}",
                        Category = $"Category {(i % 3) + 1}",
                        Image = $"/uploads/p{imgNum}.jpg", // ✅ pick random 1–4
                        Price = random.Next(10, 200),
                        Quantity = random.Next(1, 50),
                        DiscountRate = (decimal)(random.NextDouble() * 0.3),
                        CreatedBy = u.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                    productId++;
                }
            }


            await context.SaveChangesAsync();
        }
    }
}
