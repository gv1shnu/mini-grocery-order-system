using MiniGrocery.DTOs;

namespace MiniGrocery.Services
{
    public interface IOrderService
    {
        Task<PlaceOrderResult> PlaceOrderAsync(OrderRequestDto request);
    }
}
