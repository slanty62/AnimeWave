using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimeWave.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimePopularity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Animes");

            migrationBuilder.AddColumn<int>(
                name: "OpenCount",
                table: "Animes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenCount",
                table: "Animes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Animes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
