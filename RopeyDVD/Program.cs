using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;

var builder = WebApplication.CreateBuilder(args);

//get connection string from app.settings
var connectionString = builder.Configuration.GetConnectionString("AppDB");

// Add services to the container.
builder.Services.AddControllersWithViews();

//DU if DBContext
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
