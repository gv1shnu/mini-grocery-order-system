using Microsoft.EntityFrameworkCore;
using MiniGrocery.Data;
using MiniGrocery.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );

builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
