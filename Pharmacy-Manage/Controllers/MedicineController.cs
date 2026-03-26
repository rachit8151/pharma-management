using Microsoft.AspNetCore.Mvc;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;
using System.Linq;

namespace Pharmacy_Manage.Controllers
{
	public class MedicineController : Controller
	{
		private readonly ApplicationDbContext _context;

		public MedicineController(ApplicationDbContext context)
		{
			_context = context;
		}

		// VIEW MEDICINES
		public IActionResult Index(string search, string category)
		{
			var medicines = _context.Medicines.AsQueryable();

			// Search filter
			if (!string.IsNullOrEmpty(search))
			{
				medicines = medicines.Where(m =>
					m.MedicineName.Contains(search) ||
					m.Manufacturer.Contains(search));
			}

			// Category filter
			if (!string.IsNullOrEmpty(category))
			{
				medicines = medicines.Where(m => m.Category == category);
			}

			return View(medicines.ToList());
		}

		// ADD MEDICINE PAGE
		public IActionResult Create()
		{
			ViewBag.Categories = new List<string>
			{
				"Tablet",
				"Capsule",
				"Syrup",
				"Injection",
				"Cream",
				"Drops",
				"Powder",
			};

			return View();
		}

		// ADD MEDICINE SAVE
		[HttpPost]
		public IActionResult Create(Medicine medicine)
		{
			var adminEmail = HttpContext.Session.GetString("UserEmail");

			var admin = _context.Users
				.FirstOrDefault(u => u.Email == adminEmail);

			medicine.CreatedBy = admin.UserId;

			_context.Medicines.Add(medicine);
			_context.SaveChanges();

			return RedirectToAction("Index");
		}

		// EDIT MEDICINE
		public IActionResult Edit(int id)
		{
			var medicine = _context.Medicines.Find(id);
			ViewBag.Categories = new List<string>
			{
				"Tablet",
				"Capsule",
				"Syrup",
				"Injection",
				"Cream",
				"Drops",
				"Powder",
			};

			return View(medicine);
		}

		[HttpPost]
		public IActionResult Edit(Medicine medicine)
		{
			var existingMedicine = _context.Medicines
				.FirstOrDefault(m => m.MedicineId == medicine.MedicineId);

			if (existingMedicine != null)
			{
				existingMedicine.MedicineName = medicine.MedicineName;
				existingMedicine.Category = medicine.Category;
				existingMedicine.Manufacturer = medicine.Manufacturer;
				existingMedicine.UnitPrice = medicine.UnitPrice;

				_context.SaveChanges();
			}

			return RedirectToAction("Index");
		}

		// DELETE MEDICINE
		public IActionResult Delete(int id)
		{
			var medicine = _context.Medicines.Find(id);

			if (medicine != null)
			{
				_context.Medicines.Remove(medicine);
				_context.SaveChanges();
			}

			return RedirectToAction("Index");
		}

		public IActionResult UploadCsv()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> UploadCsv(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return RedirectToAction("Index");

			int inserted = 0;
			int skipped = 0;

			var adminEmail = HttpContext.Session.GetString("UserEmail");

			var admin = _context.Users
				.FirstOrDefault(u => u.Email == adminEmail);

			if (admin == null)
				return RedirectToAction("Login", "Account");

			using (var reader = new StreamReader(file.OpenReadStream()))
			{
				bool isFirstLine = true;

				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();

					if (isFirstLine)
					{
						isFirstLine = false;
						continue;
					}

					if (string.IsNullOrWhiteSpace(line)) {
						continue;
					}
						

					var values = line.Split(',');

					string medicineName = values[0].Trim();
					string category = values[1].Trim();
					string manufacturer = values[2].Trim();
					decimal price = 0;
					decimal.TryParse(values[3], out price);

					bool exists = _context.Medicines.Any(m =>
						m.MedicineName.ToLower().Trim() == medicineName.ToLower().Trim() &&
						m.Manufacturer.ToLower().Trim() == manufacturer.ToLower().Trim());

					if (exists)
					{
						skipped++;
						continue;
					}

					var medicine = new Medicine
					{
						MedicineName = medicineName,
						Category = category,
						Manufacturer = manufacturer,
						UnitPrice = price,
						CreatedBy = admin.UserId,
						CreatedAt = DateTime.Now
					};

					_context.Medicines.Add(medicine);
					inserted++;
				}

				await _context.SaveChangesAsync();
			}

			TempData["Message"] = $"{inserted} medicines inserted, {skipped} duplicates skipped.";

			return RedirectToAction("Index");
		}
	}
}