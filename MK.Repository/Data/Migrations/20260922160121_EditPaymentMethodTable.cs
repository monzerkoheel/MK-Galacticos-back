using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditPaymentMethodTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PaymentMethodTranslations");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "PaymentMethods",
                newName: "IconImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IconImageUrl",
                table: "PaymentMethods",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PaymentMethodTranslations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
