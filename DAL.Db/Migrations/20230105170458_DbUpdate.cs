using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    /// <inheritdoc />
    public partial class DbUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckersGames_CheckersOptions_CheckersOptionId",
                table: "CheckersGames");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckersGames_CheckersOptions_CheckersOptionId",
                table: "CheckersGames",
                column: "CheckersOptionId",
                principalTable: "CheckersOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckersGames_CheckersOptions_CheckersOptionId",
                table: "CheckersGames");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckersGames_CheckersOptions_CheckersOptionId",
                table: "CheckersGames",
                column: "CheckersOptionId",
                principalTable: "CheckersOptions",
                principalColumn: "Id");
        }
    }
}
