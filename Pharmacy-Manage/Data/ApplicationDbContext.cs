using Microsoft.EntityFrameworkCore;
using Pharmacy_Manage.Models;

namespace Pharmacy_Manage.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Pharmacist> Pharmacists { get; set; }
		public DbSet<Medicine> Medicines { get; set; }
		public DbSet<MedicineRequest> MedicineRequests { get; set; }
		public DbSet<PharmacistMedicine> PharmacistMedicines { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<Sale> Sales { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<MedicineRequest>()
				.HasOne(r => r.Pharmacist)
				.WithMany()
				.HasForeignKey(r => r.PharmacistId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<MedicineRequest>()
				.HasOne(r => r.Medicine)
				.WithMany()
				.HasForeignKey(r => r.MedicineId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<PharmacistMedicine>()
				.HasOne(pm => pm.Pharmacist)
				.WithMany()
				.HasForeignKey(pm => pm.PharmacistId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<PharmacistMedicine>()
				.HasOne(pm => pm.Medicine)
				.WithMany()
				.HasForeignKey(pm => pm.MedicineId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Inventory>()
				.HasOne(i => i.Pharmacist)
				.WithMany()
				.HasForeignKey(i => i.PharmacistId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<Inventory>()
				.HasOne(i => i.Medicine)
				.WithMany()
				.HasForeignKey(i => i.MedicineId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<Sale>()
				.HasOne(s => s.Pharmacist)
				.WithMany()
				.HasForeignKey(s => s.PharmacistId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Sale>()
				.HasOne(s => s.Medicine)
				.WithMany()
				.HasForeignKey(s => s.MedicineId)
				.OnDelete(DeleteBehavior.Restrict);

		}

	}

}