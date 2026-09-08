using AnimeWave.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var context =
                services.GetRequiredService<
                    ApplicationDbContext>();

            await context.Database
                .MigrateAsync();


            await SeedGenres(context);

            await SeedAdmin(services);
        }


        private static async Task SeedGenres(
            ApplicationDbContext context)
        {
            if (await context.Genres.AnyAsync())
                return;

            var genres = new[]
            {
                new Genre { Name = "Экшен" },
                new Genre { Name = "Приключения" },
                new Genre { Name = "Фэнтези" },
                new Genre { Name = "Драма" },
                new Genre { Name = "Романтика" },
                new Genre { Name = "Комедия" },
                new Genre { Name = "Триллер" },
                new Genre { Name = "Мистика" },
                new Genre { Name = "Спорт" },
                new Genre { Name = "Фантастика" }
            };

            context.Genres.AddRange(genres);

            await context.SaveChangesAsync();
        }


        private static async Task SeedAdmin(
            IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<
                    UserManager<ApplicationUser>>();


            if (!await roleManager
                .RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("Admin"));
            }


            const string adminEmail =
                "admin@animewave.local";

            const string adminPassword =
                "AnimeWave123!";


            var admin =
                await userManager.FindByEmailAsync(
                    adminEmail);


            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "Administrator"
                };

                var result =
                    await userManager.CreateAsync(
                        admin,
                        adminPassword);

                if (result.Succeeded)
                {
                    await userManager
                        .AddToRoleAsync(
                            admin,
                            "Admin");
                }
            }
        }
    }
}