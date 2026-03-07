using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_Manage.Models
{
	public class Medicine
	{
		[Key]
		public int MedicineId { get; set; }

		[Required]
		[MaxLength(150)]
		public string MedicineName { get; set; }

		[Required]
		[MaxLength(100)]
		public string Category { get; set; }

		[MaxLength(150)]
		public string Manufacturer { get; set; }

		[Required]
		[Column(TypeName = "decimal(10,2)")]
		public decimal UnitPrice { get; set; }

		[ForeignKey("User")]
		public int CreatedBy { get; set; }

		public User User { get; set; }

		public int StockQuantity { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}