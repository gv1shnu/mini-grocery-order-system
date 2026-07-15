using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniGrocery.Data;
using MiniGrocery.Models;

namespace MiniGrocery.Tests;

/// <summary>
/// Boots the real API against a throwaway SQLite file, one per test class.
/// A file (rather than in-memory) database is required: the concurrency tests
/// need several connections to contend for the same data.
/// </summary>
public class GroceryApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"minigrocery-tests-{Guid.NewGuid():N}.db");

    private string ConnectionString => $"Data Source={_dbPath};Default Timeout=30";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(ConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        // WAL lets readers and the single writer coexist instead of failing
        // outright with SQLITE_BUSY the moment two requests overlap.
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
    }

    /// <summary>Resets a product to a known stock level and clears order history.</summary>
    public async Task ResetAsync(int productId, int stock)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync("DELETE FROM Orders;");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Stock = {0} WHERE Id = {1};", stock, productId);
    }

    public async Task<int> GetStockAsync(int productId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var product = await db.Products.AsNoTracking().SingleAsync(p => p.Id == productId);
        return product.Stock;
    }

    public async Task<Order> GetOnlyOrderAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Orders.AsNoTracking().SingleAsync();
    }

    public async Task<int> CountOrdersAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Orders.CountAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        foreach (var path in new[] { _dbPath, $"{_dbPath}-wal", $"{_dbPath}-shm" })
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
