using MiniGrocery.Models;

namespace MiniGrocery.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task SaveChangesAsync();
    }
}
