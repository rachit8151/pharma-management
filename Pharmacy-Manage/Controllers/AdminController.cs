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
		public IActionResult Profile()
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
		public IActionResult Dashboard()
		{
			var expiryService = HttpContext.RequestServices.GetService<ExpiryAlertService>();
			expiryService.CheckExpiryAlerts();

			var adminEmail = HttpContext.Session.GetString("UserEmail");

			if (string.IsNullOrEmpty(adminEmail))
			{
				return RedirectToAction("Login", "Account");
			}

			var adminUser = _context.Users
				.FirstOrDefault(u => u.Email == adminEmail);

			var seasonalData = _context.MedicineRequests
				.Include(r => r.Medicine)
				.Where(r => r.Status != "rejected")
				.ToList()
				.GroupBy(r =>
				{
					int month = r.RequestDate.Month;

					if (month >= 3 && month <= 6)
						return "Summer";

					if (month >= 7 && month <= 9)
						return "Monsoon";

					return "Winter";
				})
				.Select(g => new
				{
					Season = g.Key,
					TopMedicine = g.GroupBy(x => x.Medicine.MedicineName)
						.OrderByDescending(x => x.Count())
						.Select(x => x.Key)
						.FirstOrDefault(),

					TotalRequests = g.Count()
				})
				.ToList();
			ViewBag.SeasonalData = seasonalData;
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
				.Include(r => r.Pharmacist)
				.ThenInclude(p => p.User)
				.FirstOrDefault(r => r.RequestId == id);

			if (request == null)
				return RedirectToAction("MedicineRequests");

			// Prevent duplicate approval
			if (request.Status != "pending")
			{
				TempData["Error"] = "Request already processed.";
				return RedirectToAction("MedicineRequests");
			}

			// Minimum 6 months validation
			if (expiryDate < DateTime.Now.AddMonths(6))
			{
				TempData["Error"] = "Expiry date must be minimum 6 months from today.";
				return RedirectToAction("MedicineRequests");
			}

			var medicine = request.Medicine;

			if (medicine.StockQuantity < request.Quantity)
			{
				TempData["Error"] = $"Stock low. Available stock: {medicine.StockQuantity}";
				return RedirectToAction("MedicineRequests");
			}

			// Reduce stock ONLY ONCE
			medicine.StockQuantity -= request.Quantity;

			request.Status = "approved";
			request.ExpiryDate = expiryDate;

			var email = request.Pharmacist.User.Email;
			var name = request.Pharmacist.User.FullName;

			_emailService.SendOtp(
				email,
				"Medicine Request Approved",
				$"Hello {name},\n\nYour request for {medicine.MedicineName} ({request.Quantity}) has been approved.\n\nThank you."
			);

			_context.SaveChanges();

			TempData["Message"] = "Medicine approved successfully.";

			return RedirectToAction("MedicineRequests");
		}

		//SEND DELIVERY OTP
		public IActionResult SendDeliveryOtp(int id)
		{
			var request = _context.MedicineRequests
				.Include(r => r.Medicine)
				.Include(r => r.Pharmacist)
				.ThenInclude(p => p.User)
				.FirstOrDefault(r => r.RequestId == id);

			if (request == null)
				return RedirectToAction("MedicineRequests");

			string otp = new Random().Next(100000, 999999).ToString();

			request.DeliveryOtp = otp;

			var email = request.Pharmacist.User.Email;
			var name = request.Pharmacist.User.FullName;

			_emailService.SendOtp(
				email,
				"Medicine Delivery OTP",
				$@"
				Hello {name},
				Your medicine delivery is ready.

				----------------------------------------
				📅 Request Date : {request.RequestDate:dd-MM-yyyy}
				💊 Medicine     : {request.Medicine.MedicineName}
				📦 Quantity     : {request.Quantity}
				🗓 Expiry Date  : {request.ExpiryDate:dd-MM-yyyy}
				----------------------------------------

				🔐 OTP: {otp}

				⚠️ Please share this OTP with admin to receive delivery.

				Thank you,
				Pharmacy System
				"
			);

			_context.SaveChanges();

			TempData["Message"] = "OTP Sent Successfully";

			return RedirectToAction("MedicineRequests");
		}

		//delivery verification + stock + inventory
		[HttpPost]
		public IActionResult VerifyDelivery(int id, string otp, DateTime expiryDate)
		{
			var request = _context.MedicineRequests
				.Include(r => r.Medicine)
				.FirstOrDefault(r => r.RequestId == id);

			if (request == null || request.DeliveryOtp != otp)
			{
				TempData["Error"] = "Invalid OTP";
				return RedirectToAction("MedicineRequests");
			}

			//reduce Admin stock after delivered
			//var medicine = request.Medicine;
			//if (medicine.StockQuantity < request.Quantity)
			//{
			//	TempData["Error"] = "Stock not available!";
			//	return RedirectToAction("MedicineRequests");
			//}
			//medicine.StockQuantity -= request.Quantity;

			request.Status = "delivered";
			request.IsDelivered = true;

			//Add stock
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

			//Add inventory (expiry stored here)
			_context.Inventories.Add(new Inventory
			{
				PharmacistId = request.PharmacistId,
				MedicineId = request.MedicineId,
				BatchNumber = "B" + DateTime.Now.Ticks.ToString().Substring(10),
				Quantity = request.Quantity,
				ExpiryDate = request.ExpiryDate.Value,
				LastUpdated = DateTime.Now
			});
			_context.SaveChanges();

			TempData["Message"] = "Delivered Successfully";

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
					_emailService.SendOtp(
					   user.Email,
					   "Pharmacist Account Approved",
					   $"Hello {user.FullName},\n\nYour pharmacist account has been approved by admin.\nYou can now login to the Pharmacy System.\n\nThank You."
				   );
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
					_emailService.SendOtp(
					   user.Email,
					   "Pharmacist Account Approved",
					   $"Hello {user.FullName},\n\nYour pharmacist account has been approved by admin.\nYou can now login to the Pharmacy System.\n\nThank You."
				   );
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

		//medicine request reject
		[HttpPost]
		public IActionResult RejectRequest(int requestId, string reason)
		{
			var request = _context.MedicineRequests
			   .Include(r => r.Medicine)
			   .Include(r => r.Pharmacist)
			   .ThenInclude(p => p.User)
			   .FirstOrDefault(r => r.RequestId == requestId);

			request.Status = "rejected";
			request.RejectReason = reason;

			var email = request.Pharmacist.User.Email;
			var name = request.Pharmacist.User.FullName;

			_emailService.SendOtp(
				email,
				"Medicine Request Rejected",
				$"Hello {name},\n\nYour request for {request.Medicine.MedicineName} has been rejected.\nReason: {reason}\n\nPlease contact admin."
			);

			_context.SaveChanges();

			return RedirectToAction("MedicineRequests");
		}

		public IActionResult UpdateStock(int id)
		{
			var medicine = _context.Medicines.Find(id);
			return View(medicine);
		}
		[HttpPost]
		[HttpPost]
		public IActionResult UpdateStock(int MedicineId, int StockQuantity)
		{
			var medicine = _context.Medicines.Find(MedicineId);

			if (medicine == null)
			{
				TempData["Error"] = "Medicine not found.";
				return RedirectToAction("ManageStock");
			}

			if (StockQuantity <= 0)
			{
				TempData["Error"] = "Stock quantity must be greater than 0.";
				return RedirectToAction("ManageStock");
			}

			medicine.StockQuantity += StockQuantity;

			_context.SaveChanges();

			TempData["Message"] = "Stock updated successfully.";

			return RedirectToAction("ManageStock");
		}
		public IActionResult ManageStock()
		{
			var medicines = _context.Medicines.ToList();

			return View(medicines);
		}

		public IActionResult Dispatch(int id)
		{
			var request = _context.MedicineRequests
				.FirstOrDefault(r => r.RequestId == id);

			if (request != null)
			{
				request.Status = "dispatch";

				_context.SaveChanges();

				TempData["Message"] = "Medicine dispatched successfully.";
			}

			return RedirectToAction("MedicineRequests");
		}

	}
}