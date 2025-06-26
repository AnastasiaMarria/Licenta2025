using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineSure.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes for faster restaurant searching
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_City",
                table: "Restaurants",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_PriceRange",
                table: "Restaurants",
                column: "PriceRange");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Rating",
                table: "Restaurants",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsActive",
                table: "Restaurants",
                column: "IsActive");

            // Composite index for common search combinations
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_City_IsActive",
                table: "Restaurants",
                columns: new[] { "City", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_City",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_PriceRange",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Rating",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsActive",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_City_IsActive",
                table: "Restaurants");
        }
    }
}
