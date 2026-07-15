namespace MiniGrocery.Services
{
    public enum PlaceOrderOutcome
    {
        Placed,
        InvalidQuantity,
        ProductNotFound,
        InsufficientStock
    }

    /// <summary>
    /// Why an order did or did not go through. The service reports the reason and
    /// the controller decides which HTTP status expresses it.
    /// </summary>
    public readonly record struct PlaceOrderResult(PlaceOrderOutcome Outcome, int OrderId = 0)
    {
        public static PlaceOrderResult Placed(int orderId) => new(PlaceOrderOutcome.Placed, orderId);
        public static PlaceOrderResult Failed(PlaceOrderOutcome outcome) => new(outcome);
    }
}
