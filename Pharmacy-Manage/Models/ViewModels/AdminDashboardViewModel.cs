namespace Pharmacy_Manage.Models.ViewModels
{
	public class AdminDashboardViewModel
	{
		public User Admin { get; set; }
		public List<Pharmacist> Pharmacists { get; set; }
	}
}