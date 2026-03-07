using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Models;
using Pharmacy_Manage.Services;

namespace Pharmacy_Manage.Controllers
{
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly EmailService _emailService;
		private readonly OtpService _otpService;
		private readonly PasswordHashService _passwordHashService;

		public class OcrResponse
		{
			public string licenseNumber { get; set; }
			public string expiryDate { get; set; }
			public string rawText { get; set; }
		}
		public AccountController(	
	ApplicationDbContext context,
	IHttpClientFactory httpClientFactory,
	EmailService emailService,
	OtpService otpService,
	PasswordHashService passwordHashService)
		{
			_context = context;
			_httpClientFactory = httpClientFactory;
			_emailService = emailService;
			_otpService = otpService;
			_passwordHashService = passwordHashService;
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Login(string Email, string Password)
		{
			var user = _context.Users.FirstOrDefault(u => u.Email == Email);

			if (user == null)
			{
				ViewBag.Error = "Invalid credentials";
				return View();
			}

			// Check if account locked
			if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
			{
				ViewBag.Error = "Account locked. Try again after 1 minute.";
				return View();
			}

			var hashedPassword = _passwordHashService.HashPassword(Password);
			if (user.Password == hashedPassword && user.Status == "active")
			{
				// Reset attempts
				user.FailedLoginAttempts = 0;
				user.LockoutEnd = null;

				_context.SaveChanges();

				HttpContext.Session.SetString("UserEmail", user.Email);
				HttpContext.Session.SetString("UserRole", user.Role);

				if (user.Role == "admin")
					return RedirectToAction("Dashboard", "Admin");
				else
					return RedirectToAction("Dashboard", "Pharmacist");
			}

			// Wrong password
			user.FailedLoginAttempts++;

			if (user.FailedLoginAttempts >= 3)
			{
				user.LockoutEnd = DateTime.Now.AddMinutes(1);
				user.FailedLoginAttempts = 0;

				ViewBag.Error = "Too many failed attempts. Login blocked for 1 minute.";
			}
			else
			{
				ViewBag.Error = "Invalid credentials";
			}

			_context.SaveChanges();

			return View();
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(
			string FullName,
			string Email,
			string Password,
			string StoreName,
			string StoreAddress,
			string LicenseNumber,
			DateTime LicenseIssueDate,
			DateTime LicenseExpiryDate,
			IFormFile LicenseDocument)
		{
			string fileName = null;
			string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

			if (!Directory.Exists(uploadFolder))
				Directory.CreateDirectory(uploadFolder);

			fileName = Guid.NewGuid().ToString() + Path.GetExtension(LicenseDocument.FileName);
			string filePath = Path.Combine(uploadFolder, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await LicenseDocument.CopyToAsync(stream);
			}

			var client = _httpClientFactory.CreateClient();
			var content = new MultipartFormDataContent();

			var fileStream = new FileStream(filePath, FileMode.Open);
			content.Add(new StreamContent(fileStream), "file", fileName);

			var response = await client.PostAsync("http://127.0.0.1:8000/scan-license/", content);

			string verificationStatus = "pending";

			if (response.IsSuccessStatusCode)
			{
				var jsonString = await response.Content.ReadAsStringAsync();

				var result = JsonConvert.DeserializeObject<OcrResponse>(jsonString);

				string ocrLicense = result.licenseNumber?.Trim();
				string ocrExpiry = result.expiryDate?.Trim();

				if (ocrLicense != LicenseNumber.Trim())
				{
					verificationStatus = "rejected";
				}

				if (!DateTime.TryParseExact(
						ocrExpiry,
						"dd/MM/yyyy",
						System.Globalization.CultureInfo.InvariantCulture,
						System.Globalization.DateTimeStyles.None,
						out DateTime parsedOcrDate)
					|| parsedOcrDate.Date != LicenseExpiryDate.Date)
				{
					verificationStatus = "rejected";
				}
			}
			else
			{
				verificationStatus = "pending";
			}

			var user = new User
			{
				FullName = FullName,
				Email = Email,
				Password = _passwordHashService.HashPassword(Password),
				Role = "pharmacist",
				Status = verificationStatus == "rejected" ? "rejected" : "pending"
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			var pharmacist = new Pharmacist
			{
				UserId = user.UserId,
				StoreName = StoreName,
				StoreAddress = StoreAddress,
				LicenseNumber = LicenseNumber,
				LicenseIssueDate = LicenseIssueDate,
				LicenseExpiryDate = LicenseExpiryDate,
				LicenseDocumentUrl = fileName,
				VerificationStatus = verificationStatus
			};

			_context.Pharmacists.Add(pharmacist);
			await _context.SaveChangesAsync();

			return RedirectToAction("Login");
		}

		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}

		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public IActionResult ForgotPassword(string Email)
		{
			var user = _context.Users.FirstOrDefault(u => u.Email == Email);

			if (user == null)
			{
				ViewBag.Message = "Email not found";
				return View();
			}

			string otp = _otpService.GenerateOtp();

			HttpContext.Session.SetString("ResetEmail", Email);
			HttpContext.Session.SetString("ResetOtp", otp);

			_emailService.SendOtp(
				Email,
				"Password Reset OTP",
				$"Your OTP for password reset is: {otp}"
			);

			return RedirectToAction("VerifyOtp");
		}
		[HttpGet]
		public IActionResult VerifyOtp()
		{
			return View();
		}

		[HttpPost]
		public IActionResult VerifyOtp(string otp)
		{
			var sessionOtp = HttpContext.Session.GetString("ResetOtp");

			if (otp == sessionOtp)
			{
				return RedirectToAction("ResetPassword");
			}

			ViewBag.Message = "Invalid OTP";
			return View();
		}

		[HttpGet]
		public IActionResult ResetPassword()
		{
			var email = HttpContext.Session.GetString("ResetEmail");

			if (email == null)
				return RedirectToAction("Login");

			ViewBag.Email = email;
			return View();
		}

		[HttpPost]
		public IActionResult ResetPassword(string Email, string NewPassword)
		{
			var user = _context.Users.FirstOrDefault(u => u.Email == Email);

			if (user != null)
			{
				user.Password = _passwordHashService.HashPassword(NewPassword);
				_context.SaveChanges();
			}

			return RedirectToAction("Login");
		}
	}
}