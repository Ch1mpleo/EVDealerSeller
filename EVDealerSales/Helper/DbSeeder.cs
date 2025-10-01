using EVDealerSales.BO.Enums;
using EVDealerSales.Models;
using EVDealerSales.Models.Entities;
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

        public static async Task SeedVehiclesAsync(EVDealerSalesDbContext context)
        {
            if (!await context.Vehicles.AnyAsync())
            {
                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        ModelName = "Model S",
                        TrimName = "Plaid",
                        ModelYear = 2025,
                        BasePrice = 89990M,
                        ImageUrl = "https://images.unsplash.com/photo-1580273916550-e323be2ae537?q=80&w=764&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 100M,
                        RangeKM = 637,
                        ChargingTime = 45, // minutes for 10-80%
                        TopSpeed = 322,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "iX",
                        TrimName = "M60",
                        ModelYear = 2025,
                        BasePrice = 108900M,
                        ImageUrl = "https://plus.unsplash.com/premium_photo-1664303847960-586318f59035?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 111.5M,
                        RangeKM = 561,
                        ChargingTime = 35,
                        TopSpeed = 250,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "EQS",
                        TrimName = "580 4MATIC",
                        ModelYear = 2025,
                        BasePrice = 125900M,
                        ImageUrl = "https://plus.unsplash.com/premium_photo-1683134240084-ba074973f75e?q=80&w=1595&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 107.8M,
                        RangeKM = 587,
                        ChargingTime = 31,
                        TopSpeed = 210,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "Ioniq 6",
                        TrimName = "Limited AWD",
                        ModelYear = 2025,
                        BasePrice = 52600M,
                        ImageUrl = "https://images.unsplash.com/photo-1502877338535-766e1452684a?q=80&w=1172&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 77.4M,
                        RangeKM = 509,
                        ChargingTime = 18,
                        TopSpeed = 230,
                        IsActive = true
                    }
                };

                await context.Vehicles.AddRangeAsync(vehicles);
                await context.SaveChangesAsync();
            }
        }
    }
}
