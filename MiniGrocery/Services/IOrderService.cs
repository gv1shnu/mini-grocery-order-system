using MiniGrocery.DTOs;

namespace MiniGrocery.Services
{
    public interface IOrderService
    {
        Task<bool> PlaceOrderAsync(OrderRequestDto request);
    }
}
