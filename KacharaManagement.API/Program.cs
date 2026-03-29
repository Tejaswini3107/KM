using KacharaManagement.Repository.Data;
using KacharaManagement.Repository.Interfaces;
using KacharaManagement.Repository.Repositories;
using KacharaManagement.Business.Interfaces;
using KacharaManagement.Business.Services;
using KacharaManagement.Business;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});
builder.Services.AddControllers(); // For API controllers
builder.Services.AddControllersWithViews(); // For MVC views
builder.Services.AddScoped<KacharaManagement.Repository.Interfaces.ILogEntryRepository, KacharaManagement.Repository.Repositories.LogEntryRepository>();
builder.Services.AddScoped<KacharaManagement.Repository.Interfaces.ISensorHistoryRepository, KacharaManagement.Repository.Repositories.SensorHistoryRepository>();
builder.Services.AddScoped<KacharaManagement.Repository.Interfaces.IAdminUserRepository, KacharaManagement.Repository.Repositories.AdminUserRepository>();
builder.Services.AddScoped<KacharaManagement.Business.Interfaces.IAdminService, KacharaManagement.Business.Services.AdminService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core PostgreSQL
builder.Services.AddDbContext<GothamDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI for repository and business
builder.Services.AddScoped<KacharaManagement.Business.Interfaces.IGothamService, KacharaManagement.Business.Services.GothamService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // For API endpoints
app.MapDefaultControllerRoute(); // For MVC views
app.Run();
