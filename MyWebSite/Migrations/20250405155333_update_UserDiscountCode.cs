using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebSite.Migrations
{
    /// <inheritdoc />
    public partial class update_UserDiscountCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "UserDiscountCodes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DiscountCodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDiscountCodes", x => new { x.UserId, x.DiscountCodeId });
                    table.ForeignKey(
                        name: "FK_UserDiscountCodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDiscountCodes_discountCodes_DiscountCodeId",
                        column: x => x.DiscountCodeId,
                        principalTable: "discountCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDiscountCodes_DiscountCodeId",
                table: "UserDiscountCodes",
                column: "DiscountCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDiscountCodes");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
