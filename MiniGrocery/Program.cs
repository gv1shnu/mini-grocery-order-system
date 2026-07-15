using Microsoft.EntityFrameworkCore;
using MiniGrocery.Data;
using MiniGrocery.Repositories;
using MiniGrocery.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();
app.MapControllers();
app.Run();

// Exposed so the integration tests can boot the real application via WebApplicationFactory.
public partial class Program { }
