using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;
using Pharmacy_Manage.Services;

namespace Pharmacy_Manage.Controllers
{
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;

		public AdminController(ApplicationDbContext context, EmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		// Admin Profile Dashboard
		public IActionResult Dashboard()
		{
			var adminEmail = HttpContext.Session.GetString("UserEmail");
			if (string.IsNullOrEmpty(adminEmail))
			{
				return RedirectToAction("Login", "Account");
			}
			var adminUser = _context.Users
				.FirstOrDefault(u => u.Email == adminEmail);

			return View(adminUser);
		}

		// Pharmacist Requests Page
		public IActionResult PharmacistRequests()
		{
			var pharmacists = _context.Pharmacists
				.Include(p => p.User)
				.ToList();

			return View(pharmacists);
		}

		[HttpGet]
		public JsonResult GetPendingPharmacistCount()
		{
			var count = _context.Pharmacists
				.Count(p => p.VerificationStatus == "pending");

			return Json(count);
		}

		// Approve Medicine Request
		public IActionResult Approve(int id, DateTime expiryDate)
		{
			var request = _context.MedicineRequests
				.Include(r => r.Medicine)
				.FirstOrDefault(r => r.RequestId == id);

			if (request == null)
				return RedirectToAction("MedicineRequests");

			var medicine = request.Medicine;

			if (medicine.StockQuantity >= request.Quantity)
			{
				medicine.StockQuantity -= request.Quantity;

				request.Status = "approved";
				var stock = _context.PharmacistMedicines
					.FirstOrDefault(x =>
						x.PharmacistId == request.PharmacistId &&
						x.MedicineId == request.MedicineId);

				if (stock != null)
				{
					stock.Quantity += request.Quantity;
				}
				else
				{
					var newStock = new PharmacistMedicine
					{
						PharmacistId = request.PharmacistId,
						MedicineId = request.MedicineId,
						Quantity = request.Quantity
					};

					_context.PharmacistMedicines.Add(newStock);
				}
				var inventory = new Inventory
				{
					PharmacistId = request.PharmacistId,
					MedicineId = request.MedicineId,
					BatchNumber = "B" + DateTime.Now.Ticks.ToString().Substring(10),
					Quantity = request.Quantity,
					ExpiryDate = expiryDate,
					LastUpdated = DateTime.Now
				};

				_context.Inventories.Add(inventory);
				_context.SaveChanges();
			}
			else
			{
				TempData["Error"] = $"Stock low. Available stock: {medicine.StockQuantity}";
			}

			return RedirectToAction("MedicineRequests");
		}

		//Approve pharmacist
		[HttpPost]
		public async Task<IActionResult> ApprovePharmacist(int id)
		{
			var pharmacist = _context.Pharmacists
				.FirstOrDefault(p => p.PharmacistId == id);

			if (pharmacist != null)
			{
				// Approve pharmacist
				pharmacist.VerificationStatus = "approved";

				// Activate user account
				var user = _context.Users
					.FirstOrDefault(u => u.UserId == pharmacist.UserId);

				if (user != null)
				{
					user.Status = "active";
				}

				await _context.SaveChangesAsync();
			}

			return RedirectToAction("PharmacistRequests");
		}
		// Reject Pharmacist
		[HttpPost]
		public async Task<IActionResult> Reject(int id)
		{
			var pharmacist = _context.Pharmacists
				.FirstOrDefault(p => p.PharmacistId == id);

			if (pharmacist != null)
			{
				pharmacist.VerificationStatus = "rejected";

				var user = _context.Users
					.FirstOrDefault(u => u.UserId == pharmacist.UserId);

				if (user != null)
				{
					user.Status = "rejected";
				}

				await _context.SaveChangesAsync();
			}

			return RedirectToAction("PharmacistRequests");
		}
		public IActionResult ChangeEmail()
		{
			return View();
		}
		[HttpPost]
		public IActionResult ChangeEmail(string NewEmail)
		{
			string otp = new Random().Next(100000, 999999).ToString();

			HttpContext.Session.SetString("EmailOtp", otp);
			HttpContext.Session.SetString("NewEmail", NewEmail);

			// send OTP
			_emailService.SendOtp(
				NewEmail,
				"Email Change Verification",
				$"Your OTP to verify new email is: {otp}"
			);

			return RedirectToAction("VerifyEmailOtp");
		}
		public IActionResult VerifyEmailOtp()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> VerifyEmailOtp(string otp)
		{
			var sessionOtp = HttpContext.Session.GetString("EmailOtp");
			var newEmail = HttpContext.Session.GetString("NewEmail");
			var adminEmail = HttpContext.Session.GetString("UserEmail");

			if (otp == sessionOtp)
			{
				var user = _context.Users
					.FirstOrDefault(u => u.Email == adminEmail);

				if (user != null)
				{
					user.Email = newEmail;

					await _context.SaveChangesAsync();

					HttpContext.Session.SetString("UserEmail", newEmail);
				}

				return RedirectToAction("Dashboard");
			}

			ViewBag.Message = "Invalid OTP";
			return View();
		}
		public IActionResult MedicineRequests()
		{
			var requests = _context.MedicineRequests
				.Include(r => r.Medicine)
				.Include(r => r.Pharmacist)
				.ThenInclude(p => p.User)
				.OrderByDescending(r => r.RequestDate)
				.ToList();

			return View(requests);
		}

		[HttpGet]
		public JsonResult GetPendingMedicineRequestCount()
		{
			var count = _context.MedicineRequests
				.Count(r => r.Status == "pending");

			return Json(count);
		}


		[HttpPost]
		public IActionResult RejectRequest(int requestId, string reason)
		{
			var request = _context.MedicineRequests.Find(requestId);

			request.Status = "rejected";
			request.RejectReason = reason;

			_context.SaveChanges();

			return RedirectToAction("MedicineRequests");
		}

		public IActionResult UpdateStock(int id)
		{
			var medicine = _context.Medicines.Find(id);
			return View(medicine);
		}
		[HttpPost]
		public IActionResult UpdateStock(int MedicineId, int StockQuantity)
		{
			var medicine = _context.Medicines.Find(MedicineId);

			if (medicine != null)
			{
				medicine.StockQuantity += StockQuantity;
				_context.SaveChanges();
			}

			return RedirectToAction("ManageStock");
		}
		public IActionResult ManageStock()
		{
			var medicines = _context.Medicines.ToList();

			return View(medicines);
		}

	}
}