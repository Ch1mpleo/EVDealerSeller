using EVDealerSales.Models;
using EVDealerSales.WebMVC.Architecture;
using EVDealerSales.WebMVC.Helper;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SetupIocContainer();
builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables();

builder.Services.AddRazorPages();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

try
{
    app.ApplyMigrations(app.Logger);

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EVDealerSalesDbContext>();
        await DbSeeder.SeedUsersAsync(dbContext);
        await DbSeeder.SeedVehiclesAsync(dbContext);
        await DbSeeder.SeedReportsDataAsync(dbContext);
    }
}
catch (Exception e)
{
    app.Logger.LogError(e, "An problem occurred during migration!");
}

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
// Custom start up page
// app.MapFallbackToPage("/Dashboard/Index");

// TEst test test

app.Run();
