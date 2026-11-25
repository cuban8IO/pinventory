using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pinventory.Data.Migrations
{
    /// <summary>
    /// Bauformen als eigene Tabelle (statt Freitext) und kategorie-spezifische Eigenschaften.
    /// Vorhandene Bauform-Texte werden übernommen; Standard-Bauformen und -Eigenschaften werden einmalig angelegt.
    /// </summary>
    public partial class PackagesAndAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackageId",
                table: "Parts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Options = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeDefinitions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    Mounting = table.Column<int>(type: "INTEGER", nullable: false),
                    Group = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartAttributeValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartAttributeValues_AttributeDefinitions_AttributeDefinitionId",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartAttributeValues_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_CategoryId_SortOrder",
                table: "AttributeDefinitions",
                columns: new[] { "CategoryId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Name",
                table: "Packages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartAttributeValues_AttributeDefinitionId",
                table: "PartAttributeValues",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartAttributeValues_PartId_AttributeDefinitionId",
                table: "PartAttributeValues",
                columns: new[] { "PartId", "AttributeDefinitionId" },
                unique: true);

            // ---- Startdaten ----
            migrationBuilder.Sql(SeedPackagesSql());

            // Bisherige Freitext-Bauformen übernehmen: unbekannte werden als neue Bauform angelegt.
            migrationBuilder.Sql("""
                INSERT INTO "Packages" ("Name", "Mounting", "Group", "Description", "SortOrder")
                SELECT MIN(TRIM("Package")), 2, 'Übernommen', 'aus früherer Freitext-Eingabe', 0
                FROM "Parts"
                WHERE "Package" IS NOT NULL AND TRIM("Package") <> ''
                  AND NOT EXISTS (SELECT 1 FROM "Packages" k WHERE k."Name" = TRIM("Parts"."Package"))
                GROUP BY LOWER(TRIM("Package"));
                """);
            migrationBuilder.Sql("""
                UPDATE "Parts"
                SET "PackageId" = (SELECT k."Id" FROM "Packages" k WHERE k."Name" = TRIM("Parts"."Package"))
                WHERE "Package" IS NOT NULL AND TRIM("Package") <> '';
                """);

            migrationBuilder.Sql(SeedAttributesSql());

            // ---- Alte Spalte entfernen (erst nach der Übernahme) ----
            migrationBuilder.DropColumn(
                name: "Package",
                table: "Parts");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PackageId",
                table: "Parts",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Packages_PackageId",
                table: "Parts",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Package",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Parts" SET "Package" = (SELECT k."Name" FROM "Packages" k WHERE k."Id" = "Parts"."PackageId")
                WHERE "PackageId" IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Packages_PackageId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PackageId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Parts");

            migrationBuilder.DropTable(
                name: "PartAttributeValues");

            migrationBuilder.DropTable(
                name: "AttributeDefinitions");

            migrationBuilder.DropTable(
                name: "Packages");
        }

        // =====================================================================
        // Standard-Bauformen
        // =====================================================================

        private const int SMD = 0, THT = 1, Other = 2;

        private static readonly (int Mounting, string Group, (string Name, string Description)[] Items)[] PackageSeed =
        [
            (SMD, "Chip", [
                ("01005", "0,4 × 0,2 mm · metrisch 0402"),
                ("0201", "0,6 × 0,3 mm · metrisch 0603"),
                ("0402", "1,0 × 0,5 mm · metrisch 1005"),
                ("0603", "1,6 × 0,8 mm · metrisch 1608"),
                ("0805", "2,0 × 1,25 mm · metrisch 2012"),
                ("1206", "3,2 × 1,6 mm · metrisch 3216"),
                ("1210", "3,2 × 2,5 mm · metrisch 3225"),
                ("1812", "4,5 × 3,2 mm · metrisch 4532"),
                ("2010", "5,0 × 2,5 mm · metrisch 5025"),
                ("2512", "6,3 × 3,2 mm · metrisch 6332"),
            ]),
            (SMD, "MELF", [
                ("MicroMELF", "2,2 × 1,1 mm"),
                ("MiniMELF", "3,6 × 1,4 mm · SOD-80"),
                ("MELF", "5,8 × 2,2 mm"),
            ]),
            (SMD, "Tantal", [
                ("Tantal A", "3216-18 · EIA Case A"),
                ("Tantal B", "3528-21 · EIA Case B"),
                ("Tantal C", "6032-28 · EIA Case C"),
                ("Tantal D", "7343-31 · EIA Case D"),
            ]),
            (SMD, "Elko SMD", [
                ("SMD-Elko 4×5,4", "Ø 4 mm, Höhe 5,4 mm"),
                ("SMD-Elko 5×5,4", "Ø 5 mm, Höhe 5,4 mm"),
                ("SMD-Elko 6,3×5,4", "Ø 6,3 mm, Höhe 5,4 mm"),
                ("SMD-Elko 6,3×7,7", "Ø 6,3 mm, Höhe 7,7 mm"),
                ("SMD-Elko 8×10", "Ø 8 mm, Höhe 10 mm"),
                ("SMD-Elko 10×10", "Ø 10 mm, Höhe 10 mm"),
            ]),
            (SMD, "SOD", [
                ("SOD-923", "0,8 × 0,6 mm"),
                ("SOD-523", "1,2 × 0,8 mm"),
                ("SOD-323", "1,7 × 1,25 mm"),
                ("SOD-123", "2,7 × 1,6 mm"),
                ("SOD-123F", "2,7 × 1,6 mm, flach"),
            ]),
            (SMD, "DO-214", [
                ("SMA", "DO-214AC · 4,3 × 2,6 mm"),
                ("SMB", "DO-214AA · 4,3 × 3,6 mm"),
                ("SMC", "DO-214AB · 6,9 × 5,9 mm"),
            ]),
            (SMD, "SOT", [
                ("SOT-23", "3 Pins · 2,9 × 1,3 mm"),
                ("SOT-23-5", "5 Pins · 2,9 × 1,6 mm"),
                ("SOT-23-6", "6 Pins · 2,9 × 1,6 mm"),
                ("SOT-323", "SC-70 · 3 Pins · 2,0 × 1,25 mm"),
                ("SOT-363", "SC-70-6 · 6 Pins · 2,0 × 1,25 mm"),
                ("SOT-143", "4 Pins · 2,9 × 1,3 mm"),
                ("SOT-89", "3 Pins + Kühlfahne · 4,5 × 2,5 mm"),
                ("SOT-223", "3 Pins + Kühlfahne · 6,5 × 3,5 mm"),
            ]),
            (SMD, "TO SMD", [
                ("TO-252", "DPAK"),
                ("TO-263", "D²PAK"),
                ("TO-263-5", "D²PAK, 5 Pins"),
            ]),
            (SMD, "SOIC", [
                ("SOIC-8", "SO-8 · 3,9 mm breit · Raster 1,27 mm"),
                ("SOIC-14", "3,9 mm breit · Raster 1,27 mm"),
                ("SOIC-16", "3,9 mm breit · Raster 1,27 mm"),
                ("SOIC-16W", "7,5 mm breit · Raster 1,27 mm"),
                ("SOIC-20W", "7,5 mm breit · Raster 1,27 mm"),
                ("SOIC-28W", "7,5 mm breit · Raster 1,27 mm"),
            ]),
            (SMD, "MSOP", [
                ("MSOP-8", "3 × 3 mm · Raster 0,65 mm"),
                ("MSOP-10", "3 × 3 mm · Raster 0,5 mm"),
            ]),
            (SMD, "TSSOP", [
                ("TSSOP-8", "4,4 mm breit · Raster 0,65 mm"),
                ("TSSOP-14", "4,4 mm breit · Raster 0,65 mm"),
                ("TSSOP-16", "4,4 mm breit · Raster 0,65 mm"),
                ("TSSOP-20", "4,4 mm breit · Raster 0,65 mm"),
                ("TSSOP-28", "4,4 mm breit · Raster 0,65 mm"),
            ]),
            (SMD, "SSOP", [
                ("SSOP-16", "5,3 mm breit · Raster 0,65 mm"),
                ("SSOP-20", "5,3 mm breit · Raster 0,65 mm"),
                ("SSOP-28", "5,3 mm breit · Raster 0,65 mm"),
            ]),
            (SMD, "QFP", [
                ("TQFP-32", "7 × 7 mm · Raster 0,8 mm"),
                ("TQFP-44", "10 × 10 mm · Raster 0,8 mm"),
                ("LQFP-48", "7 × 7 mm · Raster 0,5 mm"),
                ("LQFP-64", "10 × 10 mm · Raster 0,5 mm"),
                ("LQFP-100", "14 × 14 mm · Raster 0,5 mm"),
                ("LQFP-144", "20 × 20 mm · Raster 0,5 mm"),
            ]),
            (SMD, "QFN/DFN", [
                ("DFN-8", "z.B. 3 × 3 mm"),
                ("QFN-16", "z.B. 3 × 3 mm"),
                ("QFN-20", "z.B. 4 × 4 mm"),
                ("QFN-24", "z.B. 4 × 4 mm"),
                ("QFN-28", "z.B. 5 × 5 mm"),
                ("QFN-32", "z.B. 5 × 5 mm"),
                ("QFN-48", "z.B. 7 × 7 mm"),
                ("QFN-56", "z.B. 7 × 7 mm"),
            ]),
            (SMD, "LED SMD", [
                ("PLCC-2", "z.B. 3,5 × 2,8 mm"),
                ("PLCC-4", "z.B. 3,5 × 2,8 mm, RGB"),
                ("LED 2835", "2,8 × 3,5 mm"),
                ("LED 3528", "3,5 × 2,8 mm"),
                ("LED 5050", "5,0 × 5,0 mm, oft RGB"),
            ]),
            (SMD, "Quarz SMD", [
                ("Quarz 2016", "2,0 × 1,6 mm"),
                ("Quarz 3225", "3,2 × 2,5 mm"),
                ("Quarz 5032", "5,0 × 3,2 mm"),
                ("HC-49/SMD", "HC-49 für SMD-Montage"),
            ]),
            (THT, "Axial", [
                ("Axial 0204", "Widerstand 1/8–1/4 W · 3,6 × 1,6 mm"),
                ("Axial 0207", "Widerstand 1/4–1/2 W · 6,3 × 2,5 mm"),
                ("Axial 0309", "Widerstand 1/2 W · 9 × 3,2 mm"),
                ("Axial 0411", "Widerstand 1 W · 11 × 4 mm"),
                ("Axial 0617", "Widerstand 2 W · 17 × 6 mm"),
            ]),
            (THT, "DO (axial)", [
                ("DO-35", "Glas · z.B. 1N4148"),
                ("DO-41", "z.B. 1N4007"),
                ("DO-15", "z.B. P6KE"),
                ("DO-201AD", "z.B. 1N5408"),
            ]),
            (THT, "Radial", [
                ("Radial RM 2,0", "Rastermaß 2,0 mm"),
                ("Radial RM 2,5", "Rastermaß 2,5 mm"),
                ("Radial RM 3,5", "Rastermaß 3,5 mm"),
                ("Radial RM 5,0", "Rastermaß 5,0 mm"),
                ("Radial RM 7,5", "Rastermaß 7,5 mm"),
            ]),
            (THT, "TO", [
                ("TO-92", "3 Pins · Kleinsignal"),
                ("TO-126", "3 Pins · mittlere Leistung"),
                ("TO-220", "3 Pins · mit Kühlfahne"),
                ("TO-220F", "3 Pins · isoliert (FullPAK)"),
                ("TO-220-5", "5 Pins · mit Kühlfahne"),
                ("TO-247", "3 Pins · hohe Leistung"),
                ("TO-3", "Metallgehäuse"),
            ]),
            (THT, "DIP", [
                ("DIP-4", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-6", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-8", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-14", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-16", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-18", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-20", "Raster 2,54 mm · 7,62 mm breit"),
                ("DIP-24", "Raster 2,54 mm · 7,62 oder 15,24 mm breit"),
                ("DIP-28", "Raster 2,54 mm · 7,62 oder 15,24 mm breit"),
                ("DIP-40", "Raster 2,54 mm · 15,24 mm breit"),
            ]),
            (THT, "LED THT", [
                ("LED 3 mm", "T-1"),
                ("LED 5 mm", "T-1¾"),
                ("LED 8 mm", ""),
                ("LED 10 mm", ""),
            ]),
            (THT, "Quarz THT", [
                ("HC-49/U", "Standardhöhe"),
                ("HC-49/S", "niedrige Bauform"),
                ("Quarz 2×6", "zylindrisch, z.B. Uhrenquarz 32,768 kHz"),
            ]),
            (Other, "Sicherung", [
                ("5×20 mm", "Feinsicherung (Glas/Keramik)"),
                ("6,3×32 mm", "Feinsicherung"),
                ("TR5", "Kleinstsicherung, radial"),
                ("KFZ Mini", "Flachsicherung Mini"),
                ("KFZ Standard", "Flachsicherung Standard (ATO)"),
            ]),
            (Other, "Sonstige", [
                ("Modul", "Breakout-Board / fertiges Modul"),
            ]),
        ];

        private static string SeedPackagesSql()
        {
            var sql = new StringBuilder();
            foreach (var (mounting, group, items) in PackageSeed)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    var (name, description) = items[i];
                    sql.AppendLine($"""
                        INSERT INTO "Packages" ("Name", "Mounting", "Group", "Description", "SortOrder")
                        VALUES ({Q(name)}, {mounting}, {Q(group)}, {Q(description)}, {i + 1});
                        """);
                }
            }
            return sql.ToString();
        }

        // =====================================================================
        // Standard-Eigenschaften je Kategorie (Zuordnung über den Kategorienamen)
        // =====================================================================

        private const int Text = 0, Number = 1, Choice = 2;

        private static readonly (string Category, (string Name, string Unit, int Type, string Options)[] Attributes)[] AttributeSeed =
        [
            ("Widerstände", [
                ("Toleranz", null, Choice, "0,1 %;0,5 %;1 %;2 %;5 %;10 %"),
                ("Leistung", "W", Number, null),
                ("Technologie", null, Choice, "Kohleschicht;Metallschicht;Dickschicht;Dünnschicht;Drahtwiderstand;Metalloxid"),
            ]),
            ("Kondensatoren", [
                ("Spannung", "V", Number, null),
                ("Dielektrikum", null, Choice, "C0G/NP0;X7R;X5R;Y5V;Folie (MKS/MKT);Folie (MKP);Tantal;Glimmer"),
                ("Toleranz", null, Choice, "1 %;2 %;5 %;10 %;20 %;-20/+80 %"),
            ]),
            ("Elkos", [
                ("Spannung", "V", Number, null),
                ("Max. Temperatur", null, Choice, "85 °C;105 °C;125 °C"),
                ("Low-ESR", null, Choice, "Ja;Nein"),
                ("Durchmesser", "mm", Number, null),
            ]),
            ("Spulen & Induktivitäten", [
                ("Nennstrom", "A", Number, null),
                ("Sättigungsstrom", "A", Number, null),
                ("DCR", "mΩ", Number, null),
                ("Bauart", null, Choice, "Geschirmt;Ungeschirmt;Ringkern;Ferritperle"),
            ]),
            ("Potentiometer & Trimmer", [
                ("Kennlinie", null, Choice, "linear (B);logarithmisch (A);invers log. (C)"),
                ("Bauart", null, Choice, "Drehpoti;Schiebepoti;Trimmer;Spindeltrimmer"),
                ("Leistung", "W", Number, null),
            ]),
            ("Quarze & Oszillatoren", [
                ("Frequenz", "MHz", Number, null),
                ("Lastkapazität", "pF", Number, null),
                ("Typ", null, Choice, "Quarz;Oszillator;Keramikresonator"),
            ]),
            ("Dioden", [
                ("Typ", null, Choice, "Gleichrichter;Schottky;Schaltdiode;Fast Recovery;TVS;Brückengleichrichter"),
                ("Sperrspannung U_R", "V", Number, null),
                ("Durchlassstrom I_F", "A", Number, null),
                ("Vorwärtsspannung U_F", "V", Number, null),
                ("Sperrverzug t_rr", "ns", Number, null),
            ]),
            ("Z-Dioden", [
                ("Z-Spannung U_Z", "V", Number, null),
                ("Leistung", "W", Number, null),
                ("Toleranz", null, Choice, "1 %;2 %;5 %"),
            ]),
            ("Transistoren", [
                ("Typ", null, Choice, "NPN;PNP;Darlington NPN;Darlington PNP"),
                ("U_CEO", "V", Number, null),
                ("I_C", "A", Number, null),
                ("h_FE", null, Number, null),
                ("P_tot", "W", Number, null),
                ("f_T", "MHz", Number, null),
            ]),
            ("MOSFETs", [
                ("Typ", null, Choice, "N-Kanal;P-Kanal"),
                ("U_DS", "V", Number, null),
                ("I_D", "A", Number, null),
                ("R_DS(on)", "mΩ", Number, null),
                ("U_GS(th)", "V", Number, null),
                ("Logic-Level", null, Choice, "Ja;Nein"),
                ("P_tot", "W", Number, null),
            ]),
            ("Spannungsregler", [
                ("Typ", null, Choice, "Linear fest;Linear einstellbar;LDO;Schaltregler Buck;Schaltregler Boost;Buck-Boost;Referenz"),
                ("Ausgangsspannung", "V", Number, null),
                ("Max. Eingangsspannung", "V", Number, null),
                ("Ausgangsstrom", "A", Number, null),
            ]),
            ("Operationsverstärker", [
                ("Kanäle", null, Choice, "1;2;4"),
                ("Versorgung", null, Text, null),
                ("GBW", "MHz", Number, null),
                ("Rail-to-Rail", null, Choice, "Nein;Eingang;Ausgang;Ein- und Ausgang"),
            ]),
            ("Logik-ICs", [
                ("Familie", null, Choice, "74HC;74HCT;74AHC;74LVC;74LS;CD4000"),
                ("Funktion", null, Text, null),
            ]),
            ("Mikrocontroller", [
                ("Architektur", null, Choice, "AVR;ARM Cortex-M0/M0+;ARM Cortex-M3;ARM Cortex-M4;ARM Cortex-M7;ESP32 (Xtensa);RISC-V;PIC;8051"),
                ("Flash", "kB", Number, null),
                ("RAM", "kB", Number, null),
                ("Takt", "MHz", Number, null),
                ("Versorgung", null, Text, null),
            ]),
            ("LEDs", [
                ("Farbe", null, Choice, "Rot;Grün;Blau;Gelb;Orange;Weiß;Warmweiß;RGB;Infrarot;UV"),
                ("Vorwärtsspannung U_F", "V", Number, null),
                ("Strom I_F", "mA", Number, null),
                ("Helligkeit", "mcd", Number, null),
                ("Abstrahlwinkel", "°", Number, null),
            ]),
            ("Displays", [
                ("Typ", null, Choice, "LCD Zeichen;LCD Grafik;OLED;TFT;7-Segment;E-Paper"),
                ("Auflösung", null, Text, null),
                ("Diagonale", "Zoll", Number, null),
                ("Schnittstelle", null, Choice, "I²C;SPI;Parallel;UART;HDMI"),
            ]),
            ("Optokoppler", [
                ("Ausgang", null, Choice, "Transistor;Darlington;Triac;Logik;MOSFET"),
                ("Isolationsspannung", "V", Number, null),
                ("CTR", "%", Number, null),
            ]),
            ("Steckverbinder", [
                ("Rastermaß", null, Choice, "1,0 mm;1,25 mm;1,27 mm;2,0 mm;2,5 mm;2,54 mm;3,5 mm;3,81 mm;5,0 mm;5,08 mm"),
                ("Polzahl", null, Number, null),
                ("Reihen", null, Choice, "1;2;3"),
                ("Art", null, Choice, "Stecker;Buchse;Klemme"),
            ]),
            ("Schalter & Taster", [
                ("Typ", null, Choice, "Taster;Kippschalter;Schiebeschalter;DIP-Schalter;Drehschalter;Mikroschalter"),
                ("Kontakte", null, Choice, "1x Ein;1x Um;2x Um;Öffner"),
                ("Max. Strom", "A", Number, null),
            ]),
            ("Relais", [
                ("Spulenspannung", "V", Number, null),
                ("Kontakte", null, Choice, "1x Schließer;1x Um;2x Um"),
                ("Schaltstrom", "A", Number, null),
                ("Schaltspannung", "V", Number, null),
            ]),
            ("Sicherungen", [
                ("Nennstrom", "A", Number, null),
                ("Charakteristik", null, Choice, "superflink (FF);flink (F);mittelträge (M);träge (T);superträge (TT);rückstellend (PTC)"),
                ("Nennspannung", "V", Number, null),
            ]),
            ("Kabel & Draht", [
                ("Querschnitt", "mm²", Number, null),
                ("Typ", null, Choice, "Litze;Massiv;Flachband;Abgeschirmt"),
                ("Farbe", null, Text, null),
                ("Länge", "m", Number, null),
            ]),
        ];

        private static string SeedAttributesSql()
        {
            var sql = new StringBuilder();
            foreach (var (category, attributes) in AttributeSeed)
            {
                for (var i = 0; i < attributes.Length; i++)
                {
                    var (name, unit, type, options) = attributes[i];
                    sql.AppendLine($"""
                        INSERT INTO "AttributeDefinitions" ("CategoryId", "Name", "Unit", "Type", "Options", "SortOrder")
                        SELECT "Id", {Q(name)}, {Q(unit)}, {type}, {Q(options)}, {i + 1} FROM "Categories" WHERE "Name" = {Q(category)};
                        """);
                }
            }
            return sql.ToString();
        }

        /// <summary>SQL-Literal mit sicher maskierten Anführungszeichen.</summary>
        private static string Q(string value) =>
            string.IsNullOrEmpty(value) ? "NULL" : "'" + value.Replace("'", "''") + "'";
    }
}
