using Microsoft.EntityFrameworkCore;
using BladesCalc.Repositories;
using BladesCalc.Data;
using BladesCalc.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AerodynamicsDataBladesContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAerodynamicsDataBladesRepository, AerodynamicsDataBladesRepository>();
builder.Services.AddScoped<IAerodynamicService, AerodynamicService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();