using AnimeWave.Data;
using AnimeWave.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")
        )
);


builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.Password.RequiredLength = 6;

            options.Password.RequireDigit = false;

            options.Password.RequireUppercase = false;

            options.Password.RequireLowercase = false;

            options.Password.RequireNonAlphanumeric = false;

            options.User.RequireUniqueEmail = true;
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";
    });


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();

app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");


using (var scope =
       app.Services.CreateScope())
{
    var services =
        scope.ServiceProvider;

    await DbInitializer
        .InitializeAsync(services);
}


app.Run();