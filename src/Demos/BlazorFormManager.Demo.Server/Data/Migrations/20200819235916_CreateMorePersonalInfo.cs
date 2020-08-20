using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorFormManager.Demo.Server.Data.Migrations
{
    public partial class CreateMorePersonalInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgeRange",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FavouriteColor",
                table: "AspNetUsers",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FavouriteWorkingDay",
                table: "AspNetUsers",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeRange",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FavouriteColor",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FavouriteWorkingDay",
                table: "AspNetUsers");
        }
    }
}
