using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MK.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShippingMethods_Name",
                table: "ShippingMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_Governorates_CountryId_Name",
                table: "Governorates");

            migrationBuilder.DropIndex(
                name: "IX_GiftCards_Name",
                table: "GiftCards");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Name",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Collections_Name",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ShippingMethods");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LoyaltyPointTransactions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "GiftCards");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Collections");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Governorates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Governorates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEs",
                table: "Governorates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "FanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Countries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Countries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEs",
                table: "Countries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CollectionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTranslations_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponTranslations_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GiftCardTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiftCardId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCardTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiftCardTranslations_GiftCards_GiftCardId",
                        column: x => x.GiftCardId,
                        principalTable: "GiftCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPointTransactionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoyaltyPointTransactionId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "OrderItemTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemTranslations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethodTranslations_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTranslations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShippingMethodTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingMethodId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingMethodTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingMethodTranslations_ShippingMethods_ShippingMethodId",
                        column: x => x.ShippingMethodId,
                        principalTable: "ShippingMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_CountryId_NameEn",
                table: "Governorates",
                columns: new[] { "CountryId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_NameEn",
                table: "Countries",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTranslations_CollectionId_Language",
                table: "CollectionTranslations",
                columns: new[] { "CollectionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponTranslations_CouponId_Language",
                table: "CouponTranslations",
                columns: new[] { "CouponId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTranslations_GiftCardId_Language",
                table: "GiftCardTranslations",
                columns: new[] { "GiftCardId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactionTranslations_LoyaltyPointTransactionId_Language",
                table: "LoyaltyPointTransactionTranslations",
                columns: new[] { "LoyaltyPointTransactionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemTranslations_OrderItemId_Language",
                table: "OrderItemTranslations",
                columns: new[] { "OrderItemId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodTranslations_PaymentMethodId_Language",
                table: "PaymentMethodTranslations",
                columns: new[] { "PaymentMethodId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_ProductId_Language",
                table: "ProductTranslations",
                columns: new[] { "ProductId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingMethodTranslations_ShippingMethodId_Language",
                table: "ShippingMethodTranslations",
                columns: new[] { "ShippingMethodId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionTranslations");

            migrationBuilder.DropTable(
                name: "CouponTranslations");

            migrationBuilder.DropTable(
                name: "GiftCardTranslations");

            migrationBuilder.DropTable(
                name: "LoyaltyPointTransactionTranslations");

            migrationBuilder.DropTable(
                name: "OrderItemTranslations");

            migrationBuilder.DropTable(
                name: "PaymentMethodTranslations");

            migrationBuilder.DropTable(
                name: "ProductTranslations");

            migrationBuilder.DropTable(
                name: "ShippingMethodTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Governorates_CountryId_NameEn",
                table: "Governorates");

            migrationBuilder.DropIndex(
                name: "IX_Countries_NameEn",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "NameEs",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "FanRequests");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "NameEs",
                table: "Countries");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ShippingMethods",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PaymentMethods",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PaymentMethods",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LoyaltyPointTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Governorates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "GiftCards",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Coupons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Coupons",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Countries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Collections",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Collections",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingMethods_Name",
                table: "ShippingMethods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_CountryId_Name",
                table: "Governorates",
                columns: new[] { "CountryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_Name",
                table: "GiftCards",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_Name",
                table: "Collections",
                column: "Name",
                unique: true);
        }
    }
}
