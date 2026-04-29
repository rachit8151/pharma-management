using Pharmacy_Manage.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Sale
{
	[Key]
	public int SaleId { get; set; }

	[ForeignKey("PharmacistId")]
	public Pharmacist Pharmacist { get; set; }
	public int PharmacistId { get; set; }

	[ForeignKey("MedicineId")]
	public Medicine Medicine { get; set; }
	public int MedicineId { get; set; }

	[Required(ErrorMessage = "Quantity is required")]
	[Range(1, 100000, ErrorMessage = "Quantity must be greater than 0")]
	public int QuantitySold { get; set; }

	public DateTime SaleDate { get; set; } = DateTime.Now;

	public decimal UnitPrice { get; set; }

	public decimal TotalAmount { get; set; }
}