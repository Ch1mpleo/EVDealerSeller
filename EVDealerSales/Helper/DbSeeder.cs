using EVDealerSales.Models;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Enums;
using EVDealerSales.Services.Utils;
using Microsoft.EntityFrameworkCore;

namespace EVDealerSales.WebMVC.Helper
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(EVDealerSalesDbContext context)
        {
            // apply migrations if not yet applied
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.DealerManager))
            {
                var passwordHasher = new PasswordHasher();
                var manager = new User
                {
                    FullName = "Manager 1",
                    Email = "manager@gmail.com",
                    Phone = "0999000000",
                    PasswordHash = passwordHasher.HashPassword("123")!,
                    Role = RoleType.DealerManager,
                    IsActive = true
                };
                await context.Users.AddAsync(manager);
            }

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.DealerStaff))
            {
                var passwordHasher = new PasswordHasher();
                var staff = new User
                {
                    FullName = "Staff 1",
                    Email = "staff@gmail.com",
                    Phone = "0888000000",
                    PasswordHash = passwordHasher.HashPassword("123")!,
                    Role = RoleType.DealerStaff,
                    IsActive = true
                };
                await context.Users.AddAsync(staff);
            }

            await context.SaveChangesAsync();
        }
    }
}
