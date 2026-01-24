using Microsoft.AspNetCore.Mvc;
using MiniGrocery.DTOs;
using MiniGrocery.Services;

namespace MiniGrocery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(OrderRequestDto request)
        {
            var success = await _orderService.PlaceOrderAsync(request);

            if (!success)
                return BadRequest("Insufficient stock or invalid product.");

            return Ok("Order placed successfully.");
        }
    }
}
