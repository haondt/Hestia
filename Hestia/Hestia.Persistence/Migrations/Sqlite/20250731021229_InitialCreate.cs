using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hestia.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HestiaStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    HasSeededUnitConversions = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HestiaStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: true),
                    Vendor = table.Column<string>(type: "TEXT", nullable: true),
                    ServingSizeQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    ServingSizeUnit = table.Column<string>(type: "TEXT", nullable: true),
                    AlternateServingSizeQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    AlternateServingSizeUnit = table.Column<string>(type: "TEXT", nullable: true),
                    Calories = table.Column<decimal>(type: "TEXT", nullable: true),
                    FatGrams = table.Column<decimal>(type: "TEXT", nullable: true),
                    CarbGrams = table.Column<decimal>(type: "TEXT", nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "TEXT", nullable: true),
                    PackageSizeQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    PackageSizeUnit = table.Column<string>(type: "TEXT", nullable: true),
                    PackageCostDollars = table.Column<decimal>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitConversions",
                columns: table => new
                {
                    NormalizedFromUnit = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedToUnit = table.Column<string>(type: "TEXT", nullable: false),
                    FromUnit = table.Column<string>(type: "TEXT", nullable: false),
                    ToUnit = table.Column<string>(type: "TEXT", nullable: false),
                    Multiplier = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitConversions", x => new { x.NormalizedFromUnit, x.NormalizedToUnit });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HestiaStates");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "UnitConversions");
        }
    }
}
