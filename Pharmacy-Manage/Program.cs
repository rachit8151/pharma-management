using Microsoft.EntityFrameworkCore;
using Pharmacy_Manage.Data;
using Pharmacy_Manage.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSession();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<OtpService>();
builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddScoped<ExpiryAlertService>();

//Connection String for SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
