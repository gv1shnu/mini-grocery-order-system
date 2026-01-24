using MiniGrocery.Models;

namespace MiniGrocery.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task SaveChangesAsync();
    }
}
