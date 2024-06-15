using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using C.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración del DbContext con la cadena de conexión desde appsettings.json
builder.Services.AddDbContext<CContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CContext")));

// Configuración de la sesión
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true; // asegura que la cookie de sesión sea esencial
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tiempo de expiración de la sesión
});

// Configuración de la política de autorización basada en roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("RequireTeacherOrStudentRole", policy => policy.RequireRole("Maestro", "Estudiante"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilita el uso de sesiones
app.UseSession();

// Habilita la autenticación y la autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
