using Microsoft.EntityFrameworkCore;
using BladesCalc.Repositories;
using BladesCalc.Data;
using BladesCalc.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AerodynamicsDataBladesContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IAerodynamicsDataBladesRepository, AerodynamicsDataBladesRepository>();
builder.Services.AddScoped<IAerodynamicService, AerodynamicService>();

var app = builder.Build();

// Настраиваем порт только для HTTP
app.Urls.Clear();
app.Urls.Add("http://localhost:5000");

app.UseCors("AllowAll");
// Убираем HTTPS редирект
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();