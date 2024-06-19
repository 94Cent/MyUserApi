using Microsoft.EntityFrameworkCore;
using MyUser.API.Data;
using MyUser.API.Models;

namespace MyUser.API.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context= context ?? throw new ArgumentNullException(nameof(context)) ;
        }
        //method to get all products without filtering
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }
        //method to get products with filtering
        public async Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {

            //collection to start from
            var collection = _context.Products as IQueryable<Product>;
            if (!string.IsNullOrWhiteSpace(name))
            {
                name= name.Trim();
                collection = collection.Where(p => p.Name == name);
            }
            
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(a => a.Name.Contains(searchQuery)
                || (a.Description!=null && a.Description.Contains(searchQuery)));
            }
            var totalItemCount= await collection.CountAsync();
            var paginationMetadata= new PaginationMetadata(totalItemCount, pageSize, pageNumber);

          var collectionToReturn= await collection.OrderBy(p=> p.Name)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();
            return(collectionToReturn, paginationMetadata);
        }
        public async Task<Product?> GetProductAsync(int id)
        {
            return await _context.Products
                .Where(p=> p.Id == id).FirstOrDefaultAsync();
        }
        public async Task AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ProductExistsAsync(string name)
        {
            return await _context.Products.AnyAsync(p => p.Name == name);
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
        }
    }
}
