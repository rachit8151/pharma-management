using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_Manage.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicineStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Medicines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Medicines");
        }
    }
}
