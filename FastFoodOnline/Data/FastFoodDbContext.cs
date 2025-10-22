using FastFoodOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace FastFoodOnline.Data
{
    public class FastFoodDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public FastFoodDbContext(DbContextOptions<FastFoodDbContext> options)
            : base(options)
        {
        }

        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bắt buộc gọi base trước để EF thiết lập Identity
            base.OnModelCreating(modelBuilder);

            // 1. Seed Roles cố định
            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var customerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid>
                {
                    Id = adminId,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole<Guid>
                {
                    Id = customerId,
                    Name = "Customer",
                    NormalizedName = "CUSTOMER"
                }
            );

            modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Quan hệ với ApplicationUser (khai báo navigation trong ApplicationUser.UserRoles)
                b.HasOne<ApplicationUser>()
                 .WithMany(u => u.UserRoles)
                 .HasForeignKey(ur => ur.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với IdentityRole<Guid> mà không cần nav prop
                b.HasOne<IdentityRole<Guid>>()
                 .WithMany()           // <-- bỏ r => r.Users
                 .HasForeignKey(ur => ur.RoleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Cascade delete cho IdentityUserClaim<Guid>
            modelBuilder.Entity<IdentityUserClaim<Guid>>(b =>
            {
                b.HasOne<ApplicationUser>()
                 .WithMany(u => u.UserClaims)
                 .HasForeignKey(uc => uc.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Cascade delete cho IdentityUserLogin<Guid>
            modelBuilder.Entity<IdentityUserLogin<Guid>>(b =>
            {
                b.HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId });

                b.HasOne<ApplicationUser>()
                 .WithMany(u => u.UserLogins)
                 .HasForeignKey(l => l.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });


            // 5. Các cấu hình khác của app
            modelBuilder.Entity<Category>()
                .HasMany(c => c.FoodItems)
                .WithOne(fi => fi.Category)
                .HasForeignKey(fi => fi.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<ComboItem>()
                .HasKey(ci => new { ci.ComboId, ci.FoodItemId });

            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Combo)
                .WithMany(c => c.ComboItems)
                .HasForeignKey(ci => ci.ComboId);

            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.FoodItem)
                .WithMany(fi => fi.ComboItems)
                .HasForeignKey(ci => ci.FoodItemId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.FoodItem)
                .WithMany(fi => fi.OrderDetails)
                .HasForeignKey(od => od.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Combo)
                .WithMany()
                .HasForeignKey(od => od.ComboId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<FoodItem>()
                .HasIndex(fi => fi.Name);

            modelBuilder.Entity<FoodItem>()
                .HasIndex(fi => fi.CategoryId);
        }
    }
}