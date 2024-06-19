using MyUser.API.Models;

namespace MyUser.API.Services
{
    public interface IProductRepository
    {
       Task<IEnumerable<Product>> GetProductsAsync();
       Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
       Task <Product?> GetProductAsync(int id);
       Task AddProductAsync(Product product);
       Task<bool> SaveChangesAsync();
       Task<bool> ProductExistsAsync(string name);
       void DeleteProduct(Product product);
    }
}
