using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_Manage.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryOtpToMedicineRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryOtp",
                table: "MedicineRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "MedicineRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryOtp",
                table: "MedicineRequests");

            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "MedicineRequests");
        }
    }
}
