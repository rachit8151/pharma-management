using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;

namespace Pharmacy_Manage.Controllers
{
	public class PharmacistController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PharmacistController(ApplicationDbContext context)
		{
			_context = context;
		}

		// Pharmacist Dashboard
		public IActionResult Dashboard()
		{
			var email = HttpContext.Session.GetString("UserEmail");

			if (string.IsNullOrEmpty(email))
			{
				return RedirectToAction("Login", "Account");
			}
			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			return View(pharmacist);
		}
		public IActionResult Medicines()
		{
			var medicines = _context.Medicines.ToList();

			return View(medicines);
		}

		[HttpPost]
		public IActionResult RequestMedicine(int medicineId, int quantity)
		{
			var email = HttpContext.Session.GetString("UserEmail");

			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			if (pharmacist == null)
				return RedirectToAction("Login", "Account");

			var request = new MedicineRequest
			{
				MedicineId = medicineId,
				PharmacistId = pharmacist.PharmacistId,
				Quantity = quantity,
				Status = "pending",
				RequestDate = DateTime.Now
			};

			_context.MedicineRequests.Add(request);
			_context.SaveChanges();

			return RedirectToAction("Medicines");
		}
		public IActionResult MyRequests()
		{
			var email = HttpContext.Session.GetString("UserEmail");

			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			if (pharmacist == null)
				return RedirectToAction("Login", "Account");

			var requests = _context.MedicineRequests
				.Include(r => r.Medicine)
				.Where(r => r.PharmacistId == pharmacist.PharmacistId)
				.OrderByDescending(r => r.RequestDate)
				.ToList();

			return View(requests);
		}

		public IActionResult MyStock()
		{
			var email = HttpContext.Session.GetString("UserEmail");

			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			var stock = _context.PharmacistMedicines
				.Include(x => x.Medicine)
				.Where(x => x.PharmacistId == pharmacist.PharmacistId)
				.ToList();

			return View(stock);
		}

		public IActionResult Inventory()
		{
			var email = HttpContext.Session.GetString("UserEmail");

			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			if (pharmacist == null)
				return RedirectToAction("Login", "Account");

			var inventory = _context.Inventories
				.Include(i => i.Medicine)
				.Where(i => i.PharmacistId == pharmacist.PharmacistId)
				.OrderBy(i => i.ExpiryDate)
				.ToList();

			return View(inventory);
		}
	}
}