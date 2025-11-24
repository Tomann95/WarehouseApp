using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockItemUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseDocumentLines_WarehouseLocations_WarehouseLocationId",
                table: "WarehouseDocumentLines");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseDocuments_DocumentNumber",
                table: "WarehouseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseDocumentLines_WarehouseLocationId",
                table: "WarehouseDocumentLines");

            migrationBuilder.DropIndex(
                name: "IX_StockItems_ProductId",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "WarehouseLocationId",
                table: "WarehouseDocumentLines");

            migrationBuilder.DropColumn(
                name: "MinQuantity",
                table: "StockItems");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ProductId_WarehouseLocationId",
                table: "StockItems",
                columns: new[] { "ProductId", "WarehouseLocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_ProductId_WarehouseLocationId",
                table: "StockItems");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseLocationId",
                table: "WarehouseDocumentLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinQuantity",
                table: "StockItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocuments_DocumentNumber",
                table: "WarehouseDocuments",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocumentLines_WarehouseLocationId",
                table: "WarehouseDocumentLines",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ProductId",
                table: "StockItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseDocumentLines_WarehouseLocations_WarehouseLocationId",
                table: "WarehouseDocumentLines",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");
        }
    }
}
