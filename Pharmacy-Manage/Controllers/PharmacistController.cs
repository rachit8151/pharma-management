using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;
using System.Text;

namespace Pharmacy_Manage.Controllers
{
	public class PharmacistController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PharmacistController(ApplicationDbContext context)
		{
			_context = context;
		}

		// Pharmacist Profile Dashboard
		public IActionResult Profile()
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

		public IActionResult SalesAnalytics()
		{
			return View();
		}

		public JsonResult GetSalesChartData(string date)
		{
			DateTime selectedDate = DateTime.Parse(date);

			var data = _context.Medicines
				.Select(m => new
				{
					name = m.MedicineName,
					qty = _context.Sales
						.Where(s => s.MedicineId == m.MedicineId &&
									s.SaleDate.Date == selectedDate.Date)
						.Sum(s => (int?)s.QuantitySold) ?? 0
				})
				.OrderByDescending(x => x.qty)
				.ToList();

			return Json(data);
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

		[HttpGet]
		public JsonResult GetLatestRequestStatus()
		{
			var pharmacistId = HttpContext.Session.GetInt32("PharmacistId");

			var latestRequest = _context.MedicineRequests
				.Where(r => r.PharmacistId == pharmacistId)
				.OrderByDescending(r => r.RequestDate)
				.FirstOrDefault();

			if (latestRequest == null)
				return Json(new { status = "none" });

			return Json(new { status = latestRequest.Status });
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

		public IActionResult Inventory(string search, string status)
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
				.ToList();

			// SEARCH BY MEDICINE NAME
			if (!string.IsNullOrEmpty(search))
			{
				inventory = inventory
					.Where(i => i.Medicine.MedicineName
					.ToLower()
					.Contains(search.ToLower()))
					.ToList();
			}

			// STATUS FILTER
			if (!string.IsNullOrEmpty(status))
			{
				if (status == "expired")
				{
					inventory = inventory
						.Where(i => i.ExpiryDate < DateTime.Now)
						.ToList();
				}
				else if (status == "expiring")
				{
					inventory = inventory
						.Where(i =>
							i.ExpiryDate >= DateTime.Now &&
							i.ExpiryDate <= DateTime.Now.AddDays(30))
						.ToList();
				}
				else if (status == "lowstock")
				{
					inventory = inventory
						.Where(i => i.Quantity < 20)
						.ToList();
				}
			}

			// REMOVE EXPIRED STOCK
			var expiredItems = inventory
				.Where(i => i.ExpiryDate < DateTime.Now)
				.ToList();

			foreach (var item in expiredItems)
			{
				var stock = _context.PharmacistMedicines
					.FirstOrDefault(s =>
						s.PharmacistId == item.PharmacistId &&
						s.MedicineId == item.MedicineId);

				if (stock != null)
				{
					stock.Quantity -= item.Quantity;

					if (stock.Quantity < 0)
						stock.Quantity = 0;
				}
			}

			_context.SaveChanges();

			inventory = inventory
				.OrderBy(i => i.ExpiryDate)
				.ToList();

			return View(inventory);
		}

		public IActionResult DownloadSalesPivotCSV()
		{
			var email = HttpContext.Session.GetString("UserEmail");

			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			if (pharmacist == null)
				return RedirectToAction("Login", "Account");

			//STEP 1: Load medicines (ID → Name)
			var medicines = _context.Medicines.ToList();

			var medicineMap = medicines.ToDictionary(
				m => m.MedicineId,
				m => m.MedicineName
			);

			//STEP 2: Get sales data
			var sales = _context.Sales
				.Where(s => s.PharmacistId == pharmacist.PharmacistId)
				.ToList();

			//STEP 3: Group by Date
			var grouped = sales
				.GroupBy(s => s.SaleDate.Date)
				.OrderBy(g => g.Key)
				.ToList();

			//STEP 4: Build CSV
			var csv = new System.Text.StringBuilder();

			// Header
			csv.Append("Date");
			foreach (var med in medicines)
			{
				csv.Append("," + med.MedicineName);
			}
			csv.AppendLine();

			// Rows
			foreach (var day in grouped)
			{
				csv.Append(day.Key.ToString("yyyy-MM-dd"));

				foreach (var med in medicines)
				{
					var qty = day
						.Where(x => x.MedicineId == med.MedicineId)
						.Sum(x => (int?)x.QuantitySold) ?? 0;

					csv.Append("," + qty);
				}

				csv.AppendLine();
			}

			//STEP 5: Return file
			var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

			return File(bytes, "text/csv", "sales_ml_format.csv");
		}

		[HttpGet]
		public async Task<IActionResult> GetPredictionData()
		{
			using (HttpClient client = new HttpClient())
			{
				var requestData = new { days = 7 };

				var json = JsonConvert.SerializeObject(requestData);

				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("http://127.0.0.1:5000/predict", content);

				var result = await response.Content.ReadAsStringAsync();

				return Content(result, "application/json"); // ✅ correct
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetStockSuggestions()
		{
			var email = HttpContext.Session.GetString("UserEmail");

			var pharmacist = _context.Pharmacists
				.Include(p => p.User)
				.FirstOrDefault(p => p.User.Email == email);

			if (pharmacist == null)
				return RedirectToAction("Login", "Account");

			// 🔥 CALL ML API
			using (HttpClient client = new HttpClient())
			{
				var requestData = new { days = 7 };

				var json = JsonConvert.SerializeObject(requestData);

				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("http://127.0.0.1:5000/predict", content);

				var result = await response.Content.ReadAsStringAsync();

				dynamic prediction = JsonConvert.DeserializeObject(result);

				var data = prediction.data;

				// ======================
				// CALCULATE TOTAL DEMAND (7 days)
				// ======================
				Dictionary<string, int> totalDemand = new Dictionary<string, int>();

				foreach (var day in data)
				{
					foreach (var prop in day)
					{
						string key = prop.Name;

						if (key == "date") continue;

						int value = (int)prop.Value;

						if (!totalDemand.ContainsKey(key))
							totalDemand[key] = 0;

						totalDemand[key] += value;
					}
				}

				// ======================
				// GET CURRENT STOCK
				// ======================
				var stockList = _context.PharmacistMedicines
					.Include(x => x.Medicine)
					.Where(x => x.PharmacistId == pharmacist.PharmacistId)
					.ToList();

				var suggestions = new List<StockSuggestion>();

				foreach (var item in stockList)
				{
					string medName = item.Medicine.MedicineName;

					int currentStock = item.Quantity;

					int predicted = totalDemand.ContainsKey(medName)
						? totalDemand[medName]
						: 0;

					int needToBuy = predicted - currentStock;

					suggestions.Add(new StockSuggestion
					{
						Medicine = medName,
						Stock = currentStock,
						Predicted = predicted,
						Order = needToBuy > 0 ? needToBuy : 0,
						Status = needToBuy > 0 ? "LOW" : "OK"
					});
				}

				return Json(suggestions.OrderByDescending(x => x.Status == "LOW"));
			}
		}
	}
}