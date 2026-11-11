using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.EF;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Runtime.CompilerServices;


var fiksnaKultura = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = fiksnaKultura;
CultureInfo.DefaultThreadCurrentUICulture = fiksnaKultura;

static string ProjektniDirektorijum([CallerFilePath] string putanja = "") => Path.GetDirectoryName(putanja)!;
var projektniDir = ProjektniDirektorijum();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = projektniDir,
    WebRootPath = Path.Combine(projektniDir, "wwwroot")
});

// 1. Registracija DbContext-a (EF Core 9 + MS SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ASP.NET Core Identity (autentifikacija i autorizacija, bez spoljnih provajdera)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// 3. Repository patern - registracija svih repozitorijuma (interfejs -> EF implementacija)
builder.Services.AddScoped<IKategorijaRepository, KategorijaRepository>();
builder.Services.AddScoped<IAutomobilRepository, AutomobilRepository>();
builder.Services.AddScoped<INotifikacijaRepository, NotifikacijaRepository>();
builder.Services.AddScoped<IPonudaRepository, PonudaRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Seed baze (uloge, administrator, pocetni podaci) - pokrece se pri startu
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
