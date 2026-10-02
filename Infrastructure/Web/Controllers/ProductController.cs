using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : ApiControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<PagedResultDto<ProductDto>>>> GetProducts(
            [FromQuery] int? categoryId,
            [FromQuery] int? subCategoryId,
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
                SubCategoryId = subCategoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Respond(await _productService.GetProductsAsync(filterDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<ProductDto>>> GetProduct(int id) =>
            Respond(await _productService.GetProductByIdAsync(id));

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<BaseResponseDto<List<ProductDto>>>> GetProductsByCategory(int categoryId) =>
            Respond(await _productService.GetProductsByCategoryAsync(categoryId));

        [HttpGet("search")]
        public async Task<ActionResult<BaseResponseDto<List<ProductDto>>>> SearchProducts([FromQuery] string q) =>
            Respond(await _productService.SearchProductsAsync(q));

        [HttpGet("featured")]
        public async Task<ActionResult<BaseResponseDto<List<ProductDto>>>> GetFeaturedProducts() =>
            Respond(await _productService.GetFeaturedProductsAsync());

        [HttpGet("discounted")]
        public async Task<ActionResult<BaseResponseDto<List<ProductDto>>>> GetDiscountedProducts() =>
            Respond(await _productService.GetDiscountedProductsAsync());

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<ProductDto>>> CreateProduct([FromBody] ProductDto productDto) =>
            RespondCreated(await _productService.CreateProductAsync(productDto));

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<ProductDto>>> UpdateProduct(int id, [FromBody] ProductDto productDto) =>
            Respond(await _productService.UpdateProductAsync(id, productDto));

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteProduct(int id) =>
            Respond(await _productService.DeleteProductAsync(id));
    }
}
