using System.Net;
using System.Net.Http.Json;
using MiniGrocery.Models;

namespace MiniGrocery.Tests;

public class ProductsApiTests : IClassFixture<GroceryApiFactory>
{
    private readonly GroceryApiFactory _factory;

    public ProductsApiTests(GroceryApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetProducts_ReturnsSeededCatalogue()
    {
        var products = await _factory.CreateClient()
            .GetFromJsonAsync<List<Product>>("/api/products");

        Assert.NotNull(products);
        Assert.Contains(products, p => p.Name == "Rice");
        Assert.Contains(products, p => p.Name == "Milk");
        Assert.Contains(products, p => p.Name == "Bread");
    }

    [Fact]
    public async Task GetProductById_ReturnsProduct()
    {
        var product = await _factory.CreateClient()
            .GetFromJsonAsync<Product>("/api/products/2");

        Assert.NotNull(product);
        Assert.Equal("Milk", product.Name);
    }

    [Fact]
    public async Task GetProductById_ForUnknownId_ReturnsNotFound()
    {
        var response = await _factory.CreateClient().GetAsync("/api/products/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
