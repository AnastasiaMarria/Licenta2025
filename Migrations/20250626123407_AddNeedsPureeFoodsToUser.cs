using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineSure.Migrations
{
    /// <inheritdoc />
    public partial class AddNeedsPureeFoodsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsPureeFoods",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsPureeFoods",
                table: "AspNetUsers");
        }
    }
}
