using FastFoodOnline.Data;
using FastFoodOnline.Models;
using FastFoodOnline.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Thêm MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Kết nối database
builder.Services.AddDbContext<FastFoodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FastFoodDb")));

// Đăng ký PasswordHasher cho ApplicationUser
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, Pbkdf2PasswordHasher<ApplicationUser>>();

// Cấu hình Authorization policy
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
});

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<FastFoodDbContext>()
.AddDefaultTokenProviders();

// Cấu hình cookie đăng nhập
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/Account/Login";
    opts.AccessDeniedPath = "/Account/AccessDenied";
});

// ✅ Thêm cấu hình Session
builder.Services.AddDistributedMemoryCache(); // cần cho session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // thời gian sống session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed dữ liệu
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<FastFoodDbContext>();

    await db.Database.MigrateAsync(); // Đảm bảo DB cập nhật
    await DataSeeder.SeedRolesAsync(roleMgr);
    await DataSeeder.SeedAdminUserAsync(userMgr);
    await DataSeeder.SeedSampleDataAsync(db);
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Đưa Session vào pipeline
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapDefaultControllerRoute(); // xử lý /Account/Login, /Home/Index, v.v.
app.MapRazorPages();

app.Run();
