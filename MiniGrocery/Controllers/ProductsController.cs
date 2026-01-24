using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Repositories;
using MiniGrocery.Models;

namespace MiniGrocery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _repository.GetAllAsync();
            return Ok(products);
        }

        // GET: api/products/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }
}
