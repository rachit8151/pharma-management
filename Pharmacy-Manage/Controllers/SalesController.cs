using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pharmacy_Manage.Data;

public class SalesController : Controller
{
	private readonly ApplicationDbContext _context;

	public SalesController(ApplicationDbContext context)
	{
		_context = context;
	}
	public IActionResult Index()
	{
		var sales = _context.Sales
			.Include(s => s.Medicine)
			.OrderByDescending(s => s.SaleId)
			.ToList();

		return View(sales);
	}

	[HttpGet]
	public JsonResult GetSalesByDate(string date)
	{
		DateTime selectedDate = DateTime.Parse(date);

		var sales = _context.Sales
			.Include(s => s.Medicine)
			.Where(s => s.SaleDate.Date == selectedDate.Date)
			.Select(s => new
			{
				date = s.SaleDate.ToString("dd-MM-yyyy"),
				medicine = s.Medicine.MedicineName,
				qty = s.QuantitySold,
				total = s.TotalAmount
			})
			.ToList();

		return Json(sales);
	}
	public IActionResult Create()
	{
		ViewBag.Medicines = new SelectList(_context.Medicines, "MedicineId", "MedicineName");
		return View();
	}

	public JsonResult GetMedicinePrice(int id)
	{
		var medicine = _context.Medicines
			.FirstOrDefault(m => m.MedicineId == id);

		return Json(medicine?.UnitPrice ?? 0);
	}
	public JsonResult GetMedicineStock(int id)
	{
		var email = HttpContext.Session.GetString("UserEmail");

		var pharmacist = _context.Pharmacists
			.Include(p => p.User)
			.FirstOrDefault(p => p.User.Email == email);

		if (pharmacist == null)
			return Json(0);

		var stock = _context.PharmacistMedicines
			.FirstOrDefault(pm =>
				pm.PharmacistId == pharmacist.PharmacistId &&
				pm.MedicineId == id);

		return Json(stock?.Quantity ?? 0);
	}

	[HttpPost]
	public IActionResult Create(Sale sale)
	{
		ViewBag.Medicines = new SelectList(_context.Medicines, "MedicineId", "MedicineName", sale.MedicineId);

		var email = HttpContext.Session.GetString("UserEmail");

		if (string.IsNullOrEmpty(email))
			return RedirectToAction("Login", "Account");

		var pharmacist = _context.Pharmacists
			.Include(p => p.User)
			.FirstOrDefault(p => p.User.Email == email);

		if (pharmacist == null)
			return RedirectToAction("Login", "Account");

		var medicine = _context.Medicines
			.FirstOrDefault(m => m.MedicineId == sale.MedicineId);

		if (medicine == null)
		{
			ModelState.AddModelError("", "Medicine not found");
			return View(sale);
		}

		var stock = _context.PharmacistMedicines
			.FirstOrDefault(pm =>
				pm.PharmacistId == pharmacist.PharmacistId &&
				pm.MedicineId == sale.MedicineId);

		if (stock == null || stock.Quantity <= 0)
		{
			ModelState.AddModelError("", "No stock available for this medicine");
			return View(sale);
		}

		if (sale.QuantitySold > stock.Quantity)
		{
			ModelState.AddModelError("", $"Only {stock.Quantity} items available in stock");
			return View(sale);
		}

		sale.UnitPrice = medicine.UnitPrice;
		sale.TotalAmount = medicine.UnitPrice * sale.QuantitySold;
		sale.PharmacistId = pharmacist.PharmacistId;
		sale.SaleDate = DateTime.Now;

		stock.Quantity -= sale.QuantitySold;

		_context.Sales.Add(sale);
		_context.SaveChanges();

		TempData["Message"] = "Sale Added Successfully";

		return RedirectToAction("Index");
	}

	public IActionResult Upload()
	{
		return View();
	}

	[HttpPost]
	public IActionResult UploadPreview(IFormFile file)
	{
		if (file == null || file.Length == 0)
			return Content("Upload valid CSV");

		var previewList = new List<SaleUploadPreview>();

		var email = HttpContext.Session.GetString("UserEmail");

		var pharmacist = _context.Pharmacists
			.Include(p => p.User)
			.FirstOrDefault(p => p.User.Email == email);

		if (pharmacist == null)
			return RedirectToAction("Login", "Account");

		var medicines = _context.Medicines.ToList();
		var stocks = _context.PharmacistMedicines
			.Where(p => p.PharmacistId == pharmacist.PharmacistId)
			.ToList();

		using (var reader = new StreamReader(file.OpenReadStream()))
		{
			reader.ReadLine();

			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				var values = line.Split(',');

				if (values.Length < 2 || !int.TryParse(values[1], out int qty))
				{
					previewList.Add(new SaleUploadPreview
					{
						MedicineName = values[0],
						Status = "Error",
						Message = "Invalid quantity"
					});
					continue;
				}

				string name = values[0].Trim();

				var medicine = medicines
					.FirstOrDefault(m => m.MedicineName.ToLower() == name.ToLower());

				var preview = new SaleUploadPreview
				{
					MedicineName = name,
					Quantity = qty
				};

				if (medicine == null)
				{
					preview.Status = "Error";
					preview.Message = "Medicine not found";
					previewList.Add(preview);
					continue;
				}

				var stock = stocks
					.FirstOrDefault(s => s.MedicineId == medicine.MedicineId);

				if (stock == null || stock.Quantity <= 0)
				{
					preview.Status = "Error";
					preview.Message = "No stock";
				}
				else if (qty > stock.Quantity)
				{
					preview.Status = "Warning";
					preview.Message = $"Only {stock.Quantity} available";
				}
				else
				{
					preview.Status = "Valid";
					preview.Message = "OK";
				}

				preview.UnitPrice = medicine.UnitPrice;
				preview.Total = medicine.UnitPrice * qty;
				preview.MedicineId = medicine.MedicineId;

				previewList.Add(preview);
			}
		}

		return View("UploadPreview", previewList);
	}

	[HttpPost]
	public IActionResult ConfirmUpload(List<SaleUploadPreview> list)
	{
		var email = HttpContext.Session.GetString("UserEmail");

		var pharmacist = _context.Pharmacists
			.Include(p => p.User)
			.FirstOrDefault(p => p.User.Email == email);

		int success = 0;
		int skipped = 0;

		foreach (var item in list)
		{
			if (item.Status != "Valid")
			{
				skipped++;
				continue;
			}

			var medicine = _context.Medicines
				.FirstOrDefault(m => m.MedicineId == item.MedicineId);

			var stock = _context.PharmacistMedicines
				.FirstOrDefault(pm =>
					pm.PharmacistId == pharmacist.PharmacistId &&
					pm.MedicineId == item.MedicineId);

			if (stock == null || stock.Quantity < item.Quantity)
			{
				skipped++;
				continue;
			}

			var sale = new Sale
			{
				MedicineId = item.MedicineId,
				QuantitySold = item.Quantity,
				UnitPrice = medicine.UnitPrice,
				TotalAmount = medicine.UnitPrice * item.Quantity,
				PharmacistId = pharmacist.PharmacistId,
				SaleDate = DateTime.Now
			};

			stock.Quantity -= item.Quantity;

			_context.Sales.Add(sale);
			success++;
		}

		_context.SaveChanges();

		TempData["Message"] = $"Inserted: {success}, Skipped: {skipped}";

		return RedirectToAction("Index");
	}
}