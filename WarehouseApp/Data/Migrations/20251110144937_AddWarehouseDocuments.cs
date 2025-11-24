using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPartners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsSupplier = table.Column<bool>(type: "bit", nullable: false),
                    IsCustomer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPartners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceLocationId = table.Column<int>(type: "int", nullable: true),
                    TargetLocationId = table.Column<int>(type: "int", nullable: true),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseDocuments_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseDocuments_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseDocuments_WarehouseLocations_SourceLocationId",
                        column: x => x.SourceLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseDocuments_WarehouseLocations_TargetLocationId",
                        column: x => x.TargetLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseDocumentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseDocumentId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseDocumentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseDocumentLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseDocumentLines_WarehouseDocuments_WarehouseDocumentId",
                        column: x => x.WarehouseDocumentId,
                        principalTable: "WarehouseDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseDocumentLines_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocumentLines_ProductId",
                table: "WarehouseDocumentLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocumentLines_WarehouseDocumentId",
                table: "WarehouseDocumentLines",
                column: "WarehouseDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocumentLines_WarehouseLocationId",
                table: "WarehouseDocumentLines",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocuments_BusinessPartnerId",
                table: "WarehouseDocuments",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocuments_CreatedById",
                table: "WarehouseDocuments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocuments_DocumentNumber",
                table: "WarehouseDocuments",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocuments_SourceLocationId",
                table: "WarehouseDocuments",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDocuments_TargetLocationId",
                table: "WarehouseDocuments",
                column: "TargetLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseDocumentLines");

            migrationBuilder.DropTable(
                name: "WarehouseDocuments");

            migrationBuilder.DropTable(
                name: "BusinessPartners");
        }
    }
}
