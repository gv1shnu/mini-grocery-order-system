using Microsoft.EntityFrameworkCore;
using MiniGrocery.Data;
using MiniGrocery.Repositories;
using MiniGrocery.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

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

// Bring the SQLite file up to date on boot so a fresh clone runs with `dotnet run`
// alone. Fine for a seeded demo database; a real deployment would run migrations
// as a separate step rather than from application startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

// Serves the generated OpenAPI document at /openapi/v1.json.
app.MapOpenApi();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();
app.MapControllers();
app.Run();

// Exposed so the integration tests can boot the real application via WebApplicationFactory.
public partial class Program { }
