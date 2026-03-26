using Microsoft.EntityFrameworkCore;
using Pharmacy_Manage.Data;

namespace Pharmacy_Manage.Services
{
	public class ExpiryAlertService
	{
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;

		public ExpiryAlertService(ApplicationDbContext context, EmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		public void CheckExpiryAlerts()
		{
			var alertDate = DateTime.Now.AddDays(30);

			var expiring = _context.Inventories
				.Include(i => i.Medicine)
				.Include(i => i.Pharmacist)
				.ThenInclude(p => p.User)
				.Where(i => i.ExpiryDate <= alertDate && i.ExpiryDate > DateTime.Now)
				.ToList();

			foreach (var item in expiring)
			{
				var email = item.Pharmacist.User.Email;

				_emailService.SendOtp(
					email,
					"Medicine Expiry Alert",
					$"Alert!\n\nMedicine: {item.Medicine.MedicineName}\nBatch: {item.BatchNumber}\nExpiry Date: {item.ExpiryDate.ToShortDateString()}\n\nThis medicine will expire within 30 days."
				);
			}
		}
	}
}