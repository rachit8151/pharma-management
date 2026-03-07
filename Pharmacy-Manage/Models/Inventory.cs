using System;

namespace Pharmacy_Manage.Models
{
	public class Inventory
	{
		public int InventoryId { get; set; }

		public int PharmacistId { get; set; }

		public int MedicineId { get; set; }

		public string BatchNumber { get; set; }

		public int Quantity { get; set; }

		public DateTime ExpiryDate { get; set; }

		public DateTime LastUpdated { get; set; }

		public Medicine Medicine { get; set; }
	}
}