using System.ComponentModel.DataAnnotations;

namespace MiniGrocery.DTOs
{
    public class OrderRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
