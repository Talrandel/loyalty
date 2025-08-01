using System.Globalization;
using System.Text;
using Blazored.LocalStorage;
using LoyaltyApp;
using LoyaltyApp.Db;
using LoyaltyApp.Models;
using LoyaltyApp.Pages;
using LoyaltyApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", p => p.RequireRole(Role.Admin.ToString()))
    .AddPolicy("RequireEmployee", p => p.RequireRole(Role.Employee.ToString(), Role.Admin.ToString()));

services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddRazorPages();
services.AddServerSideBlazor();
services.AddBlazoredLocalStorage();
services.AddHttpClient();

services.AddScoped<JwtAuthenticationStateProvider>();
services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<ICustomerService, CustomerService>();
services.AddScoped<IActionEntryService, ActionEntryService>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var culture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    if (!db.Users.Any(u => u.UserName == "admin" && u.Role == Role.Admin))
    {
        var admin = new User { Login = "admin", UserName = "admin", Role = Role.Admin };
        admin.PasswordHash = hasher.HashPassword(admin, "admin");
        db.Users.Add(admin);
    }
    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();