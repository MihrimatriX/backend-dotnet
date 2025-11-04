using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<PagedResultDto<ProductDto>>>> GetProducts(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? searchTerm,
            [FromQuery] string sortBy = "Id",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12)
        {
            var filterDto = new ProductFilterDto
            {
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _productService.GetProductsAsync(filterDto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<ProductDto>>> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<BaseResponseDto<IEnumerable<ProductDto>>>> GetProductsByCategory(int categoryId)
        {
            var result = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<BaseResponseDto<IEnumerable<ProductDto>>>> SearchProducts([FromQuery] string q)
        {
            var result = await _productService.SearchProductsAsync(q);
            return Ok(result);
        }

        [HttpGet("featured")]
        public async Task<ActionResult<BaseResponseDto<IEnumerable<ProductDto>>>> GetFeaturedProducts()
        {
            var result = await _productService.GetFeaturedProductsAsync();
            return Ok(result);
        }

        [HttpGet("discounted")]
        public async Task<ActionResult<BaseResponseDto<IEnumerable<ProductDto>>>> GetDiscountedProducts()
        {
            var result = await _productService.GetDiscountedProductsAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<ProductDto>>> CreateProduct([FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.CreateProductAsync(productDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetProduct), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseDto<ProductDto>>> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.UpdateProductAsync(id, productDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
