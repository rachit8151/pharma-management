using Microsoft.AspNetCore.Mvc;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;
using System.Diagnostics;

namespace Pharmacy_Manage.Controllers
{
    public class HomeController : Controller
    {
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}
		public IActionResult Index()
        {
			var adminCount = _context.Users
			.Where(u => u.Role == "admin" && u.Status != "reject")
			.Count();

			var pharmacistCount = _context.Users
				.Where(u => u.Role == "pharmacist" && u.Status != "reject")
				.Count();

			ViewBag.AdminCount = adminCount;
			ViewBag.PharmacistCount = pharmacistCount;

			return View();
        }

		[HttpGet]
		public IActionResult GetUserRoleStats()
		{
			var adminCount = _context.Users
				.Where(u => u.Role == "admin" && u.Status != "reject")
				.Count();

			var pharmacistCount = _context.Users
				.Where(u => u.Role == "pharmacist" && u.Status != "reject")
				.Count();

			return Json(new
			{
				admin = adminCount,
				pharmacist = pharmacistCount
			});
		}

		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
