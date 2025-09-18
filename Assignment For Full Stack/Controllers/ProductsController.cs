using Assignment_For_Full_Stack.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace Assignment_For_Full_Stack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IImageStorageStrategy _imageStorage;

        public ProductsController(IUnitOfWork unitOfWork, IImageStorageStrategy imageStorage)
        {
            _unitOfWork = unitOfWork;
            _imageStorage = imageStorage;
            _productRepository = unitOfWork.Repository<Product>();
        }

        // GET: api/products/my-products
        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var products = await _productRepository.GetAllAsync();
            var myProducts = products.Where(p => p.CreatedBy == userId.Value)
                                    .Select(MapToDto)
                                    .ToList();

            return Ok(myProducts);
        }

        // GET: api/products
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts([FromQuery] string category = null)
        {
            var products = await _productRepository.GetAllAsync();

            var filteredProducts = products
                .Where(p => string.IsNullOrEmpty(category) || p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Select(MapToDto)
                .ToList();

            return Ok(filteredProducts);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound("Product not found");

            return Ok(MapToDto(product));
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            string imagePath = null;
            if (createProductDto.Image != null && createProductDto.Image.Length > 0)
            {
                imagePath = _imageStorage.SaveImage(createProductDto.Image);
            }


            var product = new Product
            {
                Name = createProductDto.Name,
                ProductCode = createProductDto.ProductCode,
                Category = createProductDto.Category,
                Image = imagePath,
                Price = createProductDto.Price,
                Quantity = createProductDto.Quantity,
                DiscountRate = createProductDto.DiscountRate,
                CreatedBy = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, MapToDto(product));
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound("Product not found");

            // Check if user owns the product
            if (product.CreatedBy != userId.Value)
                return Forbid("You can only update your own products");

            string imagePath = null;
            if (updateProductDto.Image != null && updateProductDto.Image.Length > 0)
            {
                imagePath = _imageStorage.SaveImage(updateProductDto.Image);
            }

            product.Name = updateProductDto.Name;
            product.ProductCode = updateProductDto.ProductCode;
            product.Category = updateProductDto.Category;
            product.Image = imagePath;
            product.Price = updateProductDto.Price;
            product.Quantity = updateProductDto.Quantity;
            product.DiscountRate = updateProductDto.DiscountRate;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.CompleteAsync();

            return Ok(MapToDto(product));
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound("Product not found");

            // Check if user owns the product
            if (product.CreatedBy != userId.Value)
                return Forbid("You can only delete your own products");

            await _productRepository.DeleteAsync(product);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        // GET: api/products/user/{userId}
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserProducts(Guid userId)
        {
            var products = await _productRepository.GetAllAsync();
            var userProducts = products.Where(p => p.CreatedBy == userId)
                                      .Select(MapToDto)
                                      .ToList();

            return Ok(userProducts);
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return null;

            return userGuid;
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                ProductCode = product.ProductCode,
                Category = product.Category,
                Image = string.IsNullOrEmpty(product.Image) ? null : $"{GetBaseUrl()}{product.Image}",
                Price = product.Price,
                Quantity = product.Quantity,
                DiscountRate = product.DiscountRate,
                CreatedBy = product.CreatedBy,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
        private string GetBaseUrl()
        {
            return $"{Request.Scheme}://{Request.Host}";
        }
    }
}
