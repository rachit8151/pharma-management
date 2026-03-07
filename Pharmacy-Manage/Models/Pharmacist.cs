using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_Manage.Models
{
	public class Pharmacist
	{
		[Key]
		public int PharmacistId { get; set; }

		[ForeignKey("User")]
		public int UserId { get; set; }

		public User User { get; set; }

		[Required]
		[MaxLength(150)]
		public string StoreName { get; set; }

		[Required]
		public string StoreAddress { get; set; }

		[Required]
		[MaxLength(50)]
		public string LicenseNumber { get; set; }

		public DateTime LicenseIssueDate { get; set; }
		public DateTime LicenseExpiryDate { get; set; }

		public string LicenseDocumentUrl { get; set; }

		public string VerificationStatus { get; set; } = "pending";
	}
}