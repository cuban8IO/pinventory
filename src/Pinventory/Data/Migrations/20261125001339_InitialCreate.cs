using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pinventory.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocations_StorageLocations_ParentId",
                        column: x => x.ParentId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Value = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Package = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    MinQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    DatasheetUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    StorageLocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Parts_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "ParentId" },
                values: new object[,]
                {
                    { 1, null, "Passive Bauelemente", null },
                    { 8, null, "Halbleiter", null },
                    { 17, null, "Optoelektronik", null },
                    { 22, null, "Elektromechanik", null },
                    { 28, null, "Module & Boards", null },
                    { 33, null, "Kabel & Draht", null },
                    { 34, null, "Mechanik & Zubehör", null },
                    { 38, null, "Sonstiges", null },
                    { 2, null, "Widerstände", 1 },
                    { 3, null, "Kondensatoren", 1 },
                    { 4, null, "Elkos", 1 },
                    { 5, null, "Spulen & Induktivitäten", 1 },
                    { 6, null, "Potentiometer & Trimmer", 1 },
                    { 7, null, "Quarze & Oszillatoren", 1 },
                    { 9, null, "Dioden", 8 },
                    { 10, null, "Z-Dioden", 8 },
                    { 11, null, "Transistoren", 8 },
                    { 12, null, "MOSFETs", 8 },
                    { 13, null, "Spannungsregler", 8 },
                    { 14, null, "Operationsverstärker", 8 },
                    { 15, null, "Logik-ICs", 8 },
                    { 16, null, "Mikrocontroller", 8 },
                    { 18, null, "LEDs", 17 },
                    { 19, null, "Displays", 17 },
                    { 20, null, "Optokoppler", 17 },
                    { 21, null, "Fotodioden & -transistoren", 17 },
                    { 23, null, "Steckverbinder", 22 },
                    { 24, null, "Schalter & Taster", 22 },
                    { 25, null, "Relais", 22 },
                    { 26, null, "Sicherungen", 22 },
                    { 27, null, "Lautsprecher & Summer", 22 },
                    { 29, null, "Entwicklungsboards", 28 },
                    { 30, null, "Sensormodule", 28 },
                    { 31, null, "Funkmodule", 28 },
                    { 32, null, "Stromversorgungsmodule", 28 },
                    { 35, null, "Schrauben & Abstandshalter", 34 },
                    { 36, null, "Gehäuse", 34 },
                    { 37, null, "Kühlkörper", 34 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId_Name",
                table: "Categories",
                columns: new[] { "ParentId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_CategoryId",
                table: "Parts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Name",
                table: "Parts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_StorageLocationId",
                table: "Parts",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_ParentId_Name",
                table: "StorageLocations",
                columns: new[] { "ParentId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "StorageLocations");
        }
    }
}
