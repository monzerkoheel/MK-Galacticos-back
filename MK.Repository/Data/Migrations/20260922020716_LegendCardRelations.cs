using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class LegendCardRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_LegendCards_GiftCardId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GiftCardMessage",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "HasGiftCard",
                table: "Orders",
                newName: "HasLegendCard");

            migrationBuilder.RenameColumn(
                name: "GiftCardPrice",
                table: "Orders",
                newName: "LegendCardPrice");

            migrationBuilder.RenameColumn(
                name: "GiftCardId",
                table: "Orders",
                newName: "LegendCardId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_GiftCardId",
                table: "Orders",
                newName: "IX_Orders_LegendCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_LegendCards_LegendCardId",
                table: "Orders",
                column: "LegendCardId",
                principalTable: "LegendCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_LegendCards_LegendCardId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "LegendCardPrice",
                table: "Orders",
                newName: "GiftCardPrice");

            migrationBuilder.RenameColumn(
                name: "LegendCardId",
                table: "Orders",
                newName: "GiftCardId");

            migrationBuilder.RenameColumn(
                name: "HasLegendCard",
                table: "Orders",
                newName: "HasGiftCard");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_LegendCardId",
                table: "Orders",
                newName: "IX_Orders_GiftCardId");

            migrationBuilder.AddColumn<string>(
                name: "GiftCardMessage",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_LegendCards_GiftCardId",
                table: "Orders",
                column: "GiftCardId",
                principalTable: "LegendCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
