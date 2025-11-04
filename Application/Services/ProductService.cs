using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Repositories;

namespace EcommerceBackend.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<BaseResponseDto<PagedResultDto<ProductDto>>> GetProductsAsync(ProductFilterDto filterDto)
        {
            try
            {
                var products = await _productRepository.GetWithFiltersAsync(
                    filterDto.CategoryId,
                    filterDto.MinPrice,
                    filterDto.MaxPrice,
                    filterDto.SearchTerm,
                    filterDto.PageNumber,
                    filterDto.PageSize,
                    filterDto.SortBy,
                    filterDto.SortOrder
                );

                var totalCount = await _productRepository.GetTotalCountAsync(
                    filterDto.CategoryId,
                    filterDto.MinPrice,
                    filterDto.MaxPrice,
                    filterDto.SearchTerm
                );

                var productDtos = products.Select(ConvertToDto).ToList();

                var pagedResult = new PagedResultDto<ProductDto>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    PageNumber = filterDto.PageNumber,
                    PageSize = filterDto.PageSize
                };

                return BaseResponseDto<PagedResultDto<ProductDto>>.SuccessResult("Products retrieved successfully", pagedResult);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PagedResultDto<ProductDto>>.ErrorResult("Error retrieving products: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return BaseResponseDto<ProductDto>.ErrorResult("Product not found");
                }

                return BaseResponseDto<ProductDto>.SuccessResult("Product retrieved successfully", ConvertToDto(product));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ProductDto>.ErrorResult("Error retrieving product: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(categoryId);
                var productDtos = products.Select(ConvertToDto);

                return BaseResponseDto<IEnumerable<ProductDto>>.SuccessResult("Products retrieved successfully", productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<IEnumerable<ProductDto>>.ErrorResult("Error retrieving products: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var products = await _productRepository.SearchAsync(searchTerm);
                var productDtos = products.Select(ConvertToDto);

                return BaseResponseDto<IEnumerable<ProductDto>>.SuccessResult("Products retrieved successfully", productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<IEnumerable<ProductDto>>.ErrorResult("Error searching products: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ProductDto>>> GetFeaturedProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetFeaturedAsync();
                var productDtos = products.Select(ConvertToDto);

                return BaseResponseDto<IEnumerable<ProductDto>>.SuccessResult("Featured products retrieved successfully", productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<IEnumerable<ProductDto>>.ErrorResult("Error retrieving featured products: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ProductDto>>> GetDiscountedProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetDiscountedAsync();
                var productDtos = products.Select(ConvertToDto);

                return BaseResponseDto<IEnumerable<ProductDto>>.SuccessResult("Discounted products retrieved successfully", productDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<IEnumerable<ProductDto>>.ErrorResult("Error retrieving discounted products: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<ProductDto>> CreateProductAsync(ProductDto productDto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
                if (category == null)
                {
                    return BaseResponseDto<ProductDto>.ErrorResult("Category not found");
                }

                var product = ConvertToEntity(productDto, category);
                var savedProduct = await _productRepository.CreateAsync(product);

                return BaseResponseDto<ProductDto>.SuccessResult("Product created successfully", ConvertToDto(savedProduct));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ProductDto>.ErrorResult("Error creating product: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<ProductDto>> UpdateProductAsync(int id, ProductDto productDto)
        {
            try
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return BaseResponseDto<ProductDto>.ErrorResult("Product not found");
                }

                var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
                if (category == null)
                {
                    return BaseResponseDto<ProductDto>.ErrorResult("Category not found");
                }

                var product = ConvertToEntity(productDto, category);
                product.Id = id;
                var updatedProduct = await _productRepository.UpdateAsync(id, product);

                if (updatedProduct == null)
                {
                    return BaseResponseDto<ProductDto>.ErrorResult("Failed to update product");
                }

                return BaseResponseDto<ProductDto>.SuccessResult("Product updated successfully", ConvertToDto(updatedProduct));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ProductDto>.ErrorResult("Error updating product: " + ex.Message);
            }
        }

        public async Task<BaseResponseDto<string>> DeleteProductAsync(int id)
        {
            try
            {
                var result = await _productRepository.DeleteAsync(id);
                if (!result)
                {
                    return BaseResponseDto<string>.ErrorResult("Product not found");
                }

                return BaseResponseDto<string>.SuccessResult("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult("Error deleting product: " + ex.Message);
            }
        }

        private ProductDto ConvertToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                UnitInStock = product.UnitInStock,
                QuantityPerUnit = product.QuantityPerUnit,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Discount = product.Discount,
                IsActive = product.IsActive
            };
        }

        private Product ConvertToEntity(ProductDto dto, Category category)
        {
            return new Product
            {
                ProductName = dto.ProductName,
                UnitPrice = dto.UnitPrice,
                UnitInStock = dto.UnitInStock,
                QuantityPerUnit = dto.QuantityPerUnit,
                CategoryId = dto.CategoryId,
                Category = category,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Discount = dto.Discount,
                IsActive = dto.IsActive
            };
        }
    }
}
