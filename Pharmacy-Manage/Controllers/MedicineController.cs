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
		public IActionResult Index()
		{
			var medicines = _context.Medicines.ToList();

			return View(medicines);
		}

		// ADD MEDICINE PAGE
		public IActionResult Create()
		{
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
	}
}