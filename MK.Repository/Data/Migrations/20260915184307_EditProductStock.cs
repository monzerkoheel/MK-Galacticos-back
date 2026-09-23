using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditProductStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductStocks_ProductId_Size",
                table: "ProductStocks");

            migrationBuilder.AddColumn<string>(
                name: "StockType",
                table: "ProductStocks",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_ProductId_Size_StockType",
                table: "ProductStocks",
                columns: new[] { "ProductId", "Size", "StockType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductStocks_ProductId_Size_StockType",
                table: "ProductStocks");

            migrationBuilder.DropColumn(
                name: "StockType",
                table: "ProductStocks");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_ProductId_Size",
                table: "ProductStocks",
                columns: new[] { "ProductId", "Size" },
                unique: true);
        }
    }
}
