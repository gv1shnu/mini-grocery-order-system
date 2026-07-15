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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PlaceOrder(OrderRequestDto request)
        {
            var result = await _orderService.PlaceOrderAsync(request);

            return result.Outcome switch
            {
                PlaceOrderOutcome.Placed => Ok(new { orderId = result.OrderId }),

                PlaceOrderOutcome.InvalidQuantity => Problem(
                    title: "Invalid quantity.",
                    detail: "Quantity must be at least 1.",
                    statusCode: StatusCodes.Status400BadRequest),

                PlaceOrderOutcome.ProductNotFound => Problem(
                    title: "Product not found.",
                    detail: $"No product exists with id {request.ProductId}.",
                    statusCode: StatusCodes.Status404NotFound),

                PlaceOrderOutcome.InsufficientStock => Problem(
                    title: "Insufficient stock.",
                    detail: "The requested quantity exceeds the stock on hand.",
                    statusCode: StatusCodes.Status409Conflict),

                _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
            };
        }
    }
}
