using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCouponsImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DarkImageUrl",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "LightImageUrl",
                table: "Coupons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DarkImageUrl",
                table: "Coupons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LightImageUrl",
                table: "Coupons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
