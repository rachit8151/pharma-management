using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_Manage.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Medicines_MedicineId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_PharmacistId",
                table: "Inventories",
                column: "PharmacistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Medicines_MedicineId",
                table: "Inventories",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Pharmacists_PharmacistId",
                table: "Inventories",
                column: "PharmacistId",
                principalTable: "Pharmacists",
                principalColumn: "PharmacistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Medicines_MedicineId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Pharmacists_PharmacistId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_PharmacistId",
                table: "Inventories");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Medicines_MedicineId",
                table: "Inventories",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
