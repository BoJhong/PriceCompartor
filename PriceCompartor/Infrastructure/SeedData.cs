using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Data;
using PriceCompartor.Models;

namespace PriceCompartor.Infrastructure
{
    public class SeedData
    {
        public static void SeedDatabase(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new IdentityRole { Name = "Admin" },
                    new IdentityRole { Name = "User" }
                );
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var hasher = new PasswordHasher<IdentityUser>();

                context.Users.AddRange(
                    new IdentityUser
                    {
                        UserName = "admin",
                        NormalizedUserName = "ADMIN",
                        Email = "",
                        PasswordHash = hasher.HashPassword(null, "admin")
                    }
                );
                context.SaveChanges();
            }

            if (!context.Platforms.Any())
            {
                context.Platforms.AddRange(
                    new Platform { Name = "Momo" },
                    new Platform { Name = "PChome" }
                );
                context.SaveChanges();
            }
        }
    }
}
