using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_ApplicationUserId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ApplicationUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "ActorType",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorType",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ApplicationUserId",
                table: "AuditLogs",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_ApplicationUserId",
                table: "AuditLogs",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
