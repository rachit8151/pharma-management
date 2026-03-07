using System.ComponentModel.DataAnnotations;

namespace Pharmacy_Manage.Models
{
	public class PharmacistMedicine
	{
		[Key]
		public int Id { get; set; }

		public int PharmacistId { get; set; }
		public Pharmacist Pharmacist { get; set; }

		public int MedicineId { get; set; }
		public Medicine Medicine { get; set; }

		public int Quantity { get; set; }
	}
}