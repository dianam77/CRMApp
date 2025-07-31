using CRMApp.Data;
using CRMApp.Models;
using CRMApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// اضافه کردن DbContext با کانکشن استرینگ
builder.Services.AddDbContext<CRMAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// پیکربندی Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<CRMAppDbContext>()
.AddDefaultTokenProviders();

// سرویس Seed کردن داده‌ها
builder.Services.AddScoped<SeedService>();

// افزودن MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// اعمال مایگریشن‌ها و Seed داده‌ها به صورت خودکار
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CRMAppDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seeder.SeedAllAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// تنظیم مسیرهای کنترلرها
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
