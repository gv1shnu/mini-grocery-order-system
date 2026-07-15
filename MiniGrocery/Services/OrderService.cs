using MiniGrocery.Data;
using MiniGrocery.DTOs;
using MiniGrocery.Models;
using MiniGrocery.Repositories;

namespace MiniGrocery.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly TimeProvider _timeProvider;

        public OrderService(
            AppDbContext context,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            TimeProvider timeProvider)
        {
            _context = context;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _timeProvider = timeProvider;
        }

        public async Task<PlaceOrderResult> PlaceOrderAsync(OrderRequestDto request)
        {
            if (request.Quantity <= 0)
            {
                return PlaceOrderResult.Failed(PlaceOrderOutcome.InvalidQuantity);
            }

            // This transaction is load-bearing, not decoration. Microsoft.Data.Sqlite
            // opens it with BEGIN IMMEDIATE, taking SQLite's write lock here and holding it
            // until commit. That is what stops two concurrent buyers from both reading the
            // same stock level and both deducting from it. Without it, 50 concurrent orders
            // drain a stock of 10 and leave the stock sitting at 9 — OrderConcurrencyTests
            // pins exactly this behaviour.
            using var transaction = await _context.Database.BeginTransactionAsync();

            var product = await _productRepository.GetByIdAsync(request.ProductId);

            if (product == null)
            {
                return PlaceOrderResult.Failed(PlaceOrderOutcome.ProductNotFound);
            }

            if (product.Stock < request.Quantity)
            {
                return PlaceOrderResult.Failed(PlaceOrderOutcome.InsufficientStock);
            }

            product.Stock -= request.Quantity;

            var order = new Order
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                TotalPrice = product.Price * request.Quantity,
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
            };

            await _orderRepository.AddAsync(order);

            // One SaveChanges flushes both the stock deduction and the new order: they
            // share the scoped DbContext, so this is a single atomic write.
            await _orderRepository.SaveChangesAsync();

            await transaction.CommitAsync();

            return PlaceOrderResult.Placed(order.Id);
        }
    }
}
