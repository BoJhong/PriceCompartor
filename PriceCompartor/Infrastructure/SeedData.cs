using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Data;
using PriceCompartor.Models;

namespace PriceCompartor.Infrastructure
{
    public class SeedData
    {
        public static void SeedDatabase(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            context.Database.Migrate();

            if (!context.Roles.Any())
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole("Sales")).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole("User")).GetAwaiter().GetResult();
                context.SaveChanges();
            }

            if (!context.Platforms.Any())
            {
                context.Platforms.AddRange(
                    new Platform { Name = "Shopee" },
                    new Platform { Name = "Momo" },
                    new Platform { Name = "PChome" }
                );
                context.SaveChanges();
            }
        }
    }
}
