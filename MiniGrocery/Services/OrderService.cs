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

        public OrderService(
            AppDbContext context,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<bool> PlaceOrderAsync(OrderRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var product = await _productRepository.GetByIdAsync(request.ProductId);

            if (product == null || product.Stock < request.Quantity)
            {
                return false;
            }

            product.Stock -= request.Quantity;

            var order = new Order
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                TotalPrice = product.Price * request.Quantity
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
    }
}
