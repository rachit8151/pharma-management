using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_Manage.Models
{
	public class MedicineRequest
	{
		[Key]
		public int RequestId { get; set; }

		public int MedicineId { get; set; }

		public int PharmacistId { get; set; }

		public int Quantity { get; set; }

		public string Status { get; set; } = "pending";

		public string? RejectReason { get; set; }

		public DateTime RequestDate { get; set; } = DateTime.Now;

		public string? DeliveryOtp { get; set; }
		public bool IsDelivered { get; set; } = false;

		public DateTime? ExpiryDate { get; set; }

		[ForeignKey("MedicineId")]
		public Medicine Medicine { get; set; }

		[ForeignKey("PharmacistId")]
		public Pharmacist Pharmacist { get; set; }
	}
}