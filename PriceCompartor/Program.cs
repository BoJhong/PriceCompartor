using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.GeminiModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SalesDb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseResponseCaching();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
var roleManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
SeedData.SeedDatabase(context, roleManager);

app.Run();
