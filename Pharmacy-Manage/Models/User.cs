using System;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_Manage.Models
{
	public class User
	{
		[Key]
		public int UserId { get; set; }

		[Required]
		[MaxLength(100)]
		public string FullName { get; set; }

		[Required]
		[MaxLength(150)]
		public string Email { get; set; }

		[Required]
		[MaxLength(255)]
		public string Password { get; set; }

		[Required]
		public string Role { get; set; } = "pharmacist";

		public string Status { get; set; } = "pending";

		public int FailedLoginAttempts { get; set; } = 0;

		public DateTime? LockoutEnd { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}