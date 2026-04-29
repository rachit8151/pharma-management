using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_Manage.Models
{
	public class Medicine
	{
		[Key]
		public int MedicineId { get; set; }

		[Required(ErrorMessage = "Medicine name is required")]
		[StringLength(150)]
		public string MedicineName { get; set; }

		[Required(ErrorMessage = "Category is required")]
		[StringLength(100)]
		public string Category { get; set; }

		[Required(ErrorMessage = "Manufacturer is required")]
		[StringLength(150)]
		public string Manufacturer { get; set; }

		[Required(ErrorMessage = "Unit price is required")]
		[Column(TypeName = "decimal(10,2)")]
		[Range(0.01, 999999)]
		public decimal UnitPrice { get; set; }

		[Required]
		[Range(0, 100000)]
		public int StockQuantity { get; set; } = 0;

		[Required(ErrorMessage = "Dosage is required")]
		[StringLength(50)]
		public string Dosage { get; set; }

		[Required(ErrorMessage = "Age group is required")]
		[StringLength(50)]
		public string AgeGroup { get; set; }

		[Required(ErrorMessage = "Usage is required")]
		[StringLength(250)]
		public string Usage { get; set; }

		[Required(ErrorMessage = "Prescription type is required")]
		[StringLength(50)]
		public string PrescriptionType { get; set; }

		[StringLength(150)]
		public string? StorageCondition { get; set; }

		[StringLength(300)]
		public string? SideEffects { get; set; }

		[Required]
		[Range(1, 1000)]
		public int UnitsPerPack { get; set; }

		[ForeignKey("User")]
		public int CreatedBy { get; set; }

		public User? User { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}