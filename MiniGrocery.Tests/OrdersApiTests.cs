using System.Net;
using System.Net.Http.Json;
using MiniGrocery.DTOs;

namespace MiniGrocery.Tests;

public class OrdersApiTests : IClassFixture<GroceryApiFactory>
{
    private const int RiceId = 1;
    private const decimal RicePrice = 50m;
    private readonly GroceryApiFactory _factory;

    public OrdersApiTests(GroceryApiFactory factory) => _factory = factory;

    private Task<HttpResponseMessage> PostOrder(int productId, int quantity) =>
        _factory.CreateClient().PostAsJsonAsync(
            "/api/orders", new OrderRequestDto { ProductId = productId, Quantity = quantity });

    [Fact]
    public async Task PlaceOrder_DeductsStock_AndRecordsTotalPrice()
    {
        await _factory.ResetAsync(RiceId, stock: 20);

        var response = await PostOrder(RiceId, 3);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(17, await _factory.GetStockAsync(RiceId));

        var order = await _factory.GetOnlyOrderAsync();
        Assert.Equal(3, order.Quantity);
        Assert.Equal(RicePrice * 3, order.TotalPrice);
    }

    [Fact]
    public async Task PlaceOrder_StampsCreatedAt()
    {
        await _factory.ResetAsync(RiceId, stock: 20);
        var before = DateTime.UtcNow;

        await PostOrder(RiceId, 1);

        var order = await _factory.GetOnlyOrderAsync();

        // Regression guard: CreatedAt was previously never assigned, so every order
        // was stamped DateTime.MinValue (year 1).
        Assert.NotEqual(default, order.CreatedAt);
        Assert.InRange(order.CreatedAt, before.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task PlaceOrder_ConsumingExactlyAllStock_Succeeds()
    {
        await _factory.ResetAsync(RiceId, stock: 4);

        var response = await PostOrder(RiceId, 4);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await _factory.GetStockAsync(RiceId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task PlaceOrder_WithNonPositiveQuantity_IsRejected_AndLeavesStockAlone(int quantity)
    {
        await _factory.ResetAsync(RiceId, stock: 20);

        var response = await PostOrder(RiceId, quantity);

        // Regression guard: a negative quantity used to pass the stock check,
        // *increase* stock, and record an order with a negative total price.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(20, await _factory.GetStockAsync(RiceId));
        Assert.Equal(0, await _factory.CountOrdersAsync());
    }

    [Fact]
    public async Task PlaceOrder_ForUnknownProduct_ReturnsNotFound()
    {
        await _factory.ResetAsync(RiceId, stock: 20);

        var response = await PostOrder(productId: 9999, quantity: 1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, await _factory.CountOrdersAsync());
    }

    [Fact]
    public async Task PlaceOrder_ExceedingStock_ReturnsConflict()
    {
        await _factory.ResetAsync(RiceId, stock: 2);

        var response = await PostOrder(RiceId, 3);

        // A missing product and an out-of-stock product are different failures and
        // used to collapse into the same 400.
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(2, await _factory.GetStockAsync(RiceId));
        Assert.Equal(0, await _factory.CountOrdersAsync());
    }
}
