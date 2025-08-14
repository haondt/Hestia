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
                    HasSeededUnitConversions = table.Column<bool>(type: "INTEGER", nullable: false),
                    NextMealPlanNumber = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "MealPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", nullable: false),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedDescription = table.Column<string>(type: "TEXT", nullable: true),
                    YieldQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    YieldUnit = table.Column<string>(type: "TEXT", nullable: true),
                    NumberOfServings = table.Column<int>(type: "INTEGER", nullable: true),
                    PrepTimeQuantity = table.Column<double>(type: "REAL", nullable: true),
                    PrepTimeUnit = table.Column<string>(type: "TEXT", nullable: true),
                    CookTimeQuantity = table.Column<double>(type: "REAL", nullable: true),
                    CookTimeUnit = table.Column<string>(type: "TEXT", nullable: true),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "FoodLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateString = table.Column<string>(type: "TEXT", nullable: false),
                    MealPlanId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodLogs_MealPlans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "MealPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MealPlanSectionDataModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MealPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlanSectionDataModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealPlanSectionDataModel_MealPlans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "MealPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodLogSectionDataModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FoodLogId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodLogSectionDataModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodLogSectionDataModel_FoodLogs_FoodLogId",
                        column: x => x.FoodLogId,
                        principalTable: "FoodLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealPlanItemDataModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IngredientId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    MealSectionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlanItemDataModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealPlanItemDataModel_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MealPlanItemDataModel_MealPlanSectionDataModel_MealSectionId",
                        column: x => x.MealSectionId,
                        principalTable: "MealPlanSectionDataModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealPlanItemDataModel_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FoodLogItemDataModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IngredientId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    FoodLogSectionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodLogItemDataModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodLogItemDataModel_FoodLogSectionDataModel_FoodLogSectionId",
                        column: x => x.FoodLogSectionId,
                        principalTable: "FoodLogSectionDataModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodLogItemDataModel_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FoodLogItemDataModel_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogItemDataModel_FoodLogSectionId",
                table: "FoodLogItemDataModel",
                column: "FoodLogSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogItemDataModel_IngredientId",
                table: "FoodLogItemDataModel",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogItemDataModel_RecipeId",
                table: "FoodLogItemDataModel",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogs_DateString",
                table: "FoodLogs",
                column: "DateString",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogs_MealPlanId",
                table: "FoodLogs",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogSectionDataModel_FoodLogId",
                table: "FoodLogSectionDataModel",
                column: "FoodLogId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanItemDataModel_IngredientId",
                table: "MealPlanItemDataModel",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanItemDataModel_MealSectionId",
                table: "MealPlanItemDataModel",
                column: "MealSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanItemDataModel_RecipeId",
                table: "MealPlanItemDataModel",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanSectionDataModel_MealPlanId",
                table: "MealPlanSectionDataModel",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientId",
                table: "RecipeIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodLogItemDataModel");

            migrationBuilder.DropTable(
                name: "HestiaStates");

            migrationBuilder.DropTable(
                name: "MealPlanItemDataModel");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "UnitConversions");

            migrationBuilder.DropTable(
                name: "FoodLogSectionDataModel");

            migrationBuilder.DropTable(
                name: "MealPlanSectionDataModel");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "FoodLogs");

            migrationBuilder.DropTable(
                name: "MealPlans");
        }
    }
}
