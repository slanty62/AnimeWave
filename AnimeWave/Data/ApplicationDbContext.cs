using AnimeWave.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }



        // =========================================================
        // TABLES
        // =========================================================

        public DbSet<Anime> Animes { get; set; }
            = null!;


        public DbSet<Genre> Genres { get; set; }
            = null!;


        public DbSet<AnimeGenre> AnimeGenres { get; set; }
            = null!;


        public DbSet<Episode> Episodes { get; set; }
            = null!;


        public DbSet<Favorite> Favorites { get; set; }
            = null!;


        public DbSet<ViewingHistory> ViewingHistories { get; set; }
            = null!;



        // =========================================================
        // DATABASE CONFIGURATION
        // =========================================================

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            // Обязательно вызываем Identity configuration
            base.OnModelCreating(builder);



            // =====================================================
            // ANIME
            // =====================================================

            builder.Entity<Anime>(entity =>
            {
                entity.HasKey(
                    a => a.Id
                );


                entity.Property(
                    a => a.Title
                )
                .IsRequired()
                .HasMaxLength(200);


                entity.Property(
                    a => a.OriginalTitle
                )
                .HasMaxLength(200);


                entity.Property(
                    a => a.Description
                )
                .HasMaxLength(3000);


                entity.Property(
                    a => a.AgeRating
                )
                .HasMaxLength(20);


                entity.Property(
                    a => a.Status
                )
                .HasMaxLength(50);


                entity.Property(
                    a => a.PosterUrl
                )
                .HasMaxLength(500);


                entity.Property(
                    a => a.BannerUrl
                )
                .HasMaxLength(500);


                // Например: 9.1
                entity.Property(
                    a => a.Rating
                )
                .HasPrecision(
                    3,
                    1
                );
            });



            // =====================================================
            // GENRE
            // =====================================================

            builder.Entity<Genre>(entity =>
            {
                entity.HasKey(
                    g => g.Id
                );


                entity.Property(
                    g => g.Name
                )
                .IsRequired()
                .HasMaxLength(100);


                // Запрещаем одинаковые названия жанров
                entity.HasIndex(
                    g => g.Name
                )
                .IsUnique();
            });



            // =====================================================
            // ANIME <-> GENRE
            //
            // MANY TO MANY
            // =====================================================

            builder.Entity<AnimeGenre>(entity =>
            {
                // Составной первичный ключ
                entity.HasKey(
                    ag => new
                    {
                        ag.AnimeId,
                        ag.GenreId
                    }
                );


                // AnimeGenre -> Anime
                entity.HasOne(
                    ag => ag.Anime
                )
                .WithMany(
                    a => a.AnimeGenres
                )
                .HasForeignKey(
                    ag => ag.AnimeId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );


                // AnimeGenre -> Genre
                entity.HasOne(
                    ag => ag.Genre
                )
                .WithMany(
                    g => g.AnimeGenres
                )
                .HasForeignKey(
                    ag => ag.GenreId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );
            });



            // =====================================================
            // EPISODE
            // =====================================================

            builder.Entity<Episode>(entity =>
            {
                entity.HasKey(
                    e => e.Id
                );


                entity.Property(
                    e => e.Title
                )
                .IsRequired()
                .HasMaxLength(200);


                entity.Property(
                    e => e.Description
                )
                .HasMaxLength(1000);


                entity.Property(
                    e => e.ThumbnailUrl
                )
                .HasMaxLength(500);


                // Episode -> Anime
                entity.HasOne(
                    e => e.Anime
                )
                .WithMany(
                    a => a.Episodes
                )
                .HasForeignKey(
                    e => e.AnimeId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );


                // У одного Anime не может быть
                // двух серий с одинаковым номером
                entity.HasIndex(
                    e => new
                    {
                        e.AnimeId,
                        e.EpisodeNumber
                    }
                )
                .IsUnique();
            });



            // =====================================================
            // FAVORITES
            // =====================================================

            builder.Entity<Favorite>(entity =>
            {
                entity.HasKey(
                    f => f.Id
                );


                // Favorite -> ApplicationUser
                //
                // ВАЖНО:
                // используем именно f.User,
                // чтобы EF не создавал UserId1
                entity.HasOne(
                    f => f.User
                )
                .WithMany()
                .HasForeignKey(
                    f => f.UserId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );


                // Favorite -> Anime
                entity.HasOne(
                    f => f.Anime
                )
                .WithMany()
                .HasForeignKey(
                    f => f.AnimeId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );


                // Один пользователь не может
                // добавить одно Anime в избранное дважды
                entity.HasIndex(
                    f => new
                    {
                        f.UserId,
                        f.AnimeId
                    }
                )
                .IsUnique();


                entity.Property(
                    f => f.CreatedAt
                )
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP"
                );
            });



            // =====================================================
            // APPLICATION USER
            // =====================================================

            builder.Entity<ApplicationUser>(entity =>
            {
                // -------------------------------------------------
                // DISPLAY NAME
                // -------------------------------------------------

                entity.Property(
                    u => u.DisplayName
                )
                .HasMaxLength(100);



                // -------------------------------------------------
                // AVATAR STYLE
                // -------------------------------------------------

                entity.Property(
                    u => u.AvatarStyle
                )
                .HasMaxLength(30)
                .HasDefaultValue(
                    "violet"
                );



                // -------------------------------------------------
                // INTERFACE THEME
                // -------------------------------------------------

                entity.Property(
                    u => u.ThemeStyle
                )
                .HasMaxLength(30)
                .HasDefaultValue(
                    "violet"
                );



                // -------------------------------------------------
                // CREATED AT
                // -------------------------------------------------

                entity.Property(
                    u => u.CreatedAt
                )
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP"
                );
            });



            // =====================================================
            // VIEWING HISTORY
            //
            // История недавно просмотренных Anime
            // =====================================================

            builder.Entity<ViewingHistory>(entity =>
            {
                entity.HasKey(
                    v => v.Id
                );


                entity.Property(
                    v => v.UserId
                )
                .IsRequired();


                entity.Property(
                    v => v.ViewedAt
                )
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP"
                );


                // ViewingHistory -> ApplicationUser
                entity.HasOne(
                    v => v.User
                )
                .WithMany(
                    u => u.ViewingHistories
                )
                .HasForeignKey(
                    v => v.UserId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );


                // ViewingHistory -> Anime
                entity.HasOne(
                    v => v.Anime
                )
                .WithMany()
                .HasForeignKey(
                    v => v.AnimeId
                )
                .OnDelete(
                    DeleteBehavior.Cascade
                );


                // Для одного пользователя каждое Anime
                // хранится в истории только один раз.
                //
                // При повторном просмотре обновляем ViewedAt.
                entity.HasIndex(
                    v => new
                    {
                        v.UserId,
                        v.AnimeId
                    }
                )
                .IsUnique();
            });
        }
    }
}