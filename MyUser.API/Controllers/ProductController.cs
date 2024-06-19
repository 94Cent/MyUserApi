using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyUser.API.Data;
using MyUser.API.Logging;
using MyUser.API.Models;
using MyUser.API.Models.Dto;
using MyUser.API.Services;
using System.Text.Json;

namespace MyUser.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogging _logger;
        private readonly IMapper _mapper;
        private const int maxProductsPageSize = 7;


        public ProductController(IProductRepository productRepository, ILogging logger, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 5)
        {
            _logger.Log("Getting all Products", "");

            if (pageSize > maxProductsPageSize)
            {
                pageSize = maxProductsPageSize;
            }

            var (products, paginationMetadata) = await _productRepository.GetProductsAsync(name, searchQuery, pageNumber, pageSize);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            Response.Headers.Add("X-Pagination",
                                   JsonSerializer.Serialize(paginationMetadata));
            return Ok(productDtos);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task <IActionResult> GetProduct(int id)
        {
            if (id == 0)
            {
                _logger.Log("Get Product Error with Id " + id, "");
                return BadRequest();
            }
            var product =await _productRepository.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductDto>(product));

        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductDto productDto)
        {
            if (productDto == null)
            {
                _logger.Log("ProductDto is null", "CreateProduct");
                return BadRequest("Product data is null.");
            }
            if (await _productRepository.ProductExistsAsync(productDto.Name))
            {
                ModelState.AddModelError("CustomError", "Name already exists.");
                _logger.Log("Product with the same name already exists: " + productDto.Name, "CreateProduct");
                return BadRequest(ModelState);
            }

            // Ensure the ID is zero since it's an auto-generated value
            if (productDto.Id > 0)
            {
                _logger.Log("ProductDto ID is greater than 0", "CreateProduct");
                return StatusCode(StatusCodes.Status500InternalServerError, "ID should not be set for new products.");
            }

            Product product = new()
            {
                // Id is not set because it's auto-generated
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price
            };

            await _productRepository.AddProductAsync(product);

            try
            {
                await _productRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Log("Error saving changes to the database: " + ex.Message, "CreateProduct");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while saving the product.");
            }

            _logger.Log("Product created with ID: " + product.Id, "CreateProduct");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }



        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            if (productDto == null || id != productDto.Id)
            {
                return BadRequest();
            }

            var productEntity = await _productRepository.GetProductAsync(id);
            if (productEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(productDto, productEntity);

            try
            {
                await _productRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Log("Error saving changes to the database: " + ex.Message, "UpdateProduct");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while saving the product.");
            }

            return NoContent();
        }


         [HttpDelete("{id:int}")]
         [ProducesResponseType(StatusCodes.Status204NoContent)]
         [ProducesResponseType(StatusCodes.Status400BadRequest)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]

         public async Task<ActionResult> DeleteProduct(int id)
         {
            var productEntity = await _productRepository.GetProductAsync(id);
            if (productEntity == null)
            {
                return NotFound();
            }
            _productRepository.DeleteProduct(productEntity);
            await _productRepository.SaveChangesAsync();
            return NoContent();
         }


    }
}


