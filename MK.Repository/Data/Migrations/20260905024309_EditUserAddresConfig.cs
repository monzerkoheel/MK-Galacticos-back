using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditUserAddresConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_UserId",
                table: "UserAddresses");

            migrationBuilder.AlterColumn<string>(
                name: "AddressName",
                table: "UserAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_AddressName",
                table: "UserAddresses",
                columns: new[] { "UserId", "AddressName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_UserId_AddressName",
                table: "UserAddresses");

            migrationBuilder.AlterColumn<string>(
                name: "AddressName",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId",
                table: "UserAddresses",
                column: "UserId");
        }
    }
}
