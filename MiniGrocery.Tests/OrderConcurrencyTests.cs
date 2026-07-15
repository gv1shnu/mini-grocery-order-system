using System.Net;
using System.Net.Http.Json;
using MiniGrocery.DTOs;
using Xunit.Abstractions;

namespace MiniGrocery.Tests;

public class OrderConcurrencyTests : IClassFixture<GroceryApiFactory>
{
    private const int RiceId = 1;
    private readonly GroceryApiFactory _factory;
    private readonly ITestOutputHelper _output;

    public OrderConcurrencyTests(GroceryApiFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    /// <summary>
    /// The core inventory invariant: when more buyers than stock arrive at once,
    /// exactly as many orders as there is stock may succeed. Never more.
    /// </summary>
    [Fact]
    public async Task ConcurrentOrders_NeverSellMoreThanStock()
    {
        const int stock = 10;
        const int buyers = 50;

        await _factory.ResetAsync(RiceId, stock);

        // A barrier so every request hits the endpoint at genuinely the same
        // moment, rather than trickling in as the loop spawns them.
        // RunContinuationsAsynchronously matters: without it, SetResult would
        // resume each waiter inline on the calling thread, serialising them.
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var attempts = Enumerable.Range(0, buyers).Select(_ => Task.Run(async () =>
        {
            var client = _factory.CreateClient();
            await gate.Task;
            var response = await client.PostAsJsonAsync(
                "/api/orders", new OrderRequestDto { ProductId = RiceId, Quantity = 1 });
            return response.StatusCode;
        })).ToArray();

        gate.SetResult();
        var results = await Task.WhenAll(attempts);

        foreach (var group in results.GroupBy(s => s).OrderByDescending(g => g.Count()))
            _output.WriteLine($"{(int)group.Key} {group.Key}: {group.Count()}");

        var succeeded = results.Count(s => s == HttpStatusCode.OK);
        var remainingStock = await _factory.GetStockAsync(RiceId);
        var ordersRecorded = await _factory.CountOrdersAsync();

        _output.WriteLine($"succeeded={succeeded} remainingStock={remainingStock} ordersRecorded={ordersRecorded}");

        Assert.Equal(stock, succeeded);
        Assert.Equal(0, remainingStock);
        Assert.Equal(stock, ordersRecorded);
    }
}
