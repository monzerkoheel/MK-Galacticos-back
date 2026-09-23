using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditLoyaltyPointLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyPointTransactionTranslations");

            migrationBuilder.AddColumn<int>(
                name: "CouponId",
                table: "LoyaltyPointTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FanRequestId",
                table: "LoyaltyPointTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "LoyaltyPointTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "LoyaltyPointTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "LoyaltyPointTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LifetimeEarnedLoyaltyPoints",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "LoyaltyPointTransactions");

            migrationBuilder.DropColumn(
                name: "FanRequestId",
                table: "LoyaltyPointTransactions");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "LoyaltyPointTransactions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "LoyaltyPointTransactions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "LoyaltyPointTransactions");

            migrationBuilder.DropColumn(
                name: "LifetimeEarnedLoyaltyPoints",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "LoyaltyPointTransactionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoyaltyPointTransactionId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPointTransactionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyPointTransactionTranslations_LoyaltyPointTransactions_LoyaltyPointTransactionId",
                        column: x => x.LoyaltyPointTransactionId,
                        principalTable: "LoyaltyPointTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactionTranslations_LoyaltyPointTransactionId_Language",
                table: "LoyaltyPointTransactionTranslations",
                columns: new[] { "LoyaltyPointTransactionId", "Language" },
                unique: true);
        }
    }
}
