using FastFoodOnline.Models;
using Microsoft.AspNetCore.Identity;
using FastFoodOnline.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using static FastFoodOnline.Models.FoodItem;

namespace FastFoodOnline.Data
{
    public static class DataSeeder
    {
        // 1. Seed Roles bằng RoleManager
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleMgr)
        {
            foreach (var roleName in new[] { "Admin", "Customer" })
            {
                if (!await roleMgr.RoleExistsAsync(roleName))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Seed Admin User bằng UserManager
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userMgr)
        {
            var adminEmail = "admin@fastfood.com";

            // 1. Tìm qua UserManager (đã dùng NormalizedEmail nội bộ)
            var admin = await userMgr.FindByEmailAsync(adminEmail);

            // 2. Fallback: query trực tiếp trên NormalizedEmail
            if (admin == null)
            {
                var normEmail = adminEmail.ToUpperInvariant();
                admin = userMgr.Users
                    .SingleOrDefault(u => u.NormalizedEmail == normEmail);
            }

            // 3. Nếu vẫn chưa có thì tạo mới
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Super Admin",
                    PhoneNumber = "0123456789",
                    Address = "Headquarters",
                    DateOfBirth = DateTime.Parse("1990-01-01")
                };

                await userMgr.CreateAsync(admin, "Admin@123");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }



        // 3. Seed dữ liệu mẫu cho Category, FoodItem, Combo
        public static async Task SeedSampleDataAsync(FastFoodDbContext context)
        {
            // Category
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Burger" },
                    new Category { Name = "Drink" }
                );
                await context.SaveChangesAsync();
            }

            // FoodItem
            if (!context.FoodItems.Any())
            {
                var burgerCat = context.Categories.First(c => c.Name == "Burger");
                var drinkCat = context.Categories.First(c => c.Name == "Drink");

                context.FoodItems.AddRange(
                    new FoodItem
                    {
                        Name = "Classic Burger",
                        Description = "Bánh mì thịt bò",
                        Price = 5.99m,
                        CategoryId = burgerCat.Id,
                        ImageUrl = "https://tse1.mm.bing.net/th/id/OIP.mu5NCOkccrQBBW5OZlGQeQHaHa",
                        Status = ItemStatus.Available
                    },
                    new FoodItem
                    {
                        Name = "Coke",
                        Description = "Nước ngọt có ga",
                        Price = 1.99m,
                        CategoryId = drinkCat.Id,
                        ImageUrl = "",
                        Status = ItemStatus.Available
                    }
                );
                await context.SaveChangesAsync();
            }

            // Combo
            if (!context.Combos.Any())
            {
                var combo = new Combo
                {
                    Name = "Burger + Coke",
                    Description = "Tiết kiệm với combo",
                    Price = 7.50m
                };
                context.Combos.Add(combo);
                await context.SaveChangesAsync();

                var burger = context.FoodItems.First(f => f.Name == "Classic Burger");
                var coke = context.FoodItems.First(f => f.Name == "Coke");

                context.ComboItems.AddRange(
                    new ComboItem { ComboId = combo.Id, FoodItemId = burger.Id, Quantity = 1 },
                    new ComboItem { ComboId = combo.Id, FoodItemId = coke.Id, Quantity = 1 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
