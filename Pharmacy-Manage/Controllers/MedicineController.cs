using Microsoft.AspNetCore.Mvc;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;

namespace Pharmacy_Manage.Controllers
{
	public class MedicineController : Controller
	{
		private readonly ApplicationDbContext _context;

		public MedicineController(ApplicationDbContext context)
		{
			_context = context;
		}

		// =========================
		// VIEW MEDICINES
		// =========================
		public IActionResult Index(string search, string category, string ageGroup, string prescriptionType)
		{
			var medicines = _context.Medicines.AsQueryable();

			// Search
			if (!string.IsNullOrEmpty(search))
			{
				medicines = medicines.Where(m =>
					m.MedicineName.Contains(search) ||
					m.Manufacturer.Contains(search));
			}

			// Category Filter
			if (!string.IsNullOrEmpty(category))
			{
				medicines = medicines.Where(m => m.Category == category);
			}

			// Age Group Filter
			if (!string.IsNullOrEmpty(ageGroup))
			{
				medicines = medicines.Where(m => m.AgeGroup == ageGroup);
			}

			// Prescription Filter
			if (!string.IsNullOrEmpty(prescriptionType))
			{
				medicines = medicines.Where(m => m.PrescriptionType == prescriptionType);
			}

			return View(medicines.ToList());
		}

		// =========================
		// CREATE PAGE
		// =========================
		public IActionResult Create()
		{
			LoadCategories();
			return View();
		}

		// =========================
		// CREATE SAVE
		// =========================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Medicine medicine)
		{
			LoadCategories();

			if (!ModelState.IsValid)
			{
				return View(medicine);
			}

			var adminEmail = HttpContext.Session.GetString("UserEmail");

			var admin = _context.Users
				.FirstOrDefault(u => u.Email == adminEmail);

			if (admin == null)
			{
				return RedirectToAction("Login", "Account");
			}

			// Duplicate Check
			bool exists = _context.Medicines.Any(m =>
				m.MedicineName.ToLower().Trim() == medicine.MedicineName.ToLower().Trim() &&
				m.Manufacturer.ToLower().Trim() == medicine.Manufacturer.ToLower().Trim());

			if (exists)
			{
				ModelState.AddModelError("", "Medicine already exists.");
				return View(medicine);
			}

			medicine.CreatedBy = admin.UserId;
			medicine.CreatedAt = DateTime.Now;

			_context.Medicines.Add(medicine);
			_context.SaveChanges();

			TempData["Message"] = "Medicine added successfully.";

			return RedirectToAction("Index");
		}

		// =========================
		// EDIT PAGE
		// =========================
		public IActionResult Edit(int id)
		{
			var medicine = _context.Medicines.Find(id);

			if (medicine == null)
			{
				return NotFound();
			}

			LoadCategories();

			return View(medicine);
		}

		// =========================
		// EDIT SAVE
		// =========================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Medicine medicine)
		{
			LoadCategories();

			if (!ModelState.IsValid)
			{
				return View(medicine);
			}

			var existingMedicine = _context.Medicines
				.FirstOrDefault(m => m.MedicineId == medicine.MedicineId);

			if (existingMedicine == null)
			{
				return NotFound();
			}

			existingMedicine.MedicineName = medicine.MedicineName;
			existingMedicine.Category = medicine.Category;
			existingMedicine.Manufacturer = medicine.Manufacturer;
			existingMedicine.UnitPrice = medicine.UnitPrice;
			existingMedicine.StockQuantity = medicine.StockQuantity;
			existingMedicine.Dosage = medicine.Dosage;
			existingMedicine.AgeGroup = medicine.AgeGroup;
			existingMedicine.Usage = medicine.Usage;
			existingMedicine.PrescriptionType = medicine.PrescriptionType;
			existingMedicine.SideEffects = medicine.SideEffects;
			existingMedicine.StorageCondition = medicine.StorageCondition;
			existingMedicine.UnitsPerPack = medicine.UnitsPerPack;

			_context.SaveChanges();

			TempData["Message"] = "Medicine updated successfully.";

			return RedirectToAction("Index");
		}

		// =========================
		// DELETE
		// =========================
		public IActionResult Delete(int id)
		{
			var medicine = _context.Medicines.Find(id);

			if (medicine != null)
			{
				_context.Medicines.Remove(medicine);
				_context.SaveChanges();

				TempData["Message"] = "Medicine deleted successfully.";
			}

			return RedirectToAction("Index");
		}

		// =========================
		// CSV PAGE
		// =========================
		public IActionResult UploadCsv()
		{
			return View();
		}

		// =========================
		// CSV UPLOAD
		// =========================
		[HttpPost]
		public async Task<IActionResult> UploadCsv(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return RedirectToAction("Index");
			}

			int inserted = 0;
			int skipped = 0;

			var adminEmail = HttpContext.Session.GetString("UserEmail");

			var admin = _context.Users
				.FirstOrDefault(u => u.Email == adminEmail);

			if (admin == null)
			{
				return RedirectToAction("Login", "Account");
			}

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

					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					var values = line.Split(',');

					if (values.Length < 11)
					{
						continue;
					}

					string medicineName = values[0].Trim();
					string category = values[1].Trim();
					string manufacturer = values[2].Trim();
					decimal price = decimal.Parse(values[3].Trim());
					string dosage = values[4].Trim();
					string ageGroup = values[5].Trim();
					string usage = values[6].Trim();
					string prescriptionType = values[7].Trim();
					string storageCondition = values[8].Trim();
					string sideEffects = values[9].Trim();
					int unitsPerPack = int.Parse(values[10].Trim());

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
						StockQuantity = 0,
						Dosage = dosage,
						AgeGroup = ageGroup,
						Usage = usage,
						PrescriptionType = prescriptionType,
						StorageCondition = storageCondition,
						SideEffects = sideEffects,
						UnitsPerPack = unitsPerPack,
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

		// =========================
		// CATEGORY HELPER
		// =========================
		private void LoadCategories()
		{
			ViewBag.Categories = new List<string>
			{
				"Tablet",
				"Capsule",
				"Syrup",
				"Injection",
				"Cream",
				"Drops",
				"Powder"
			};
		}
	}
}
