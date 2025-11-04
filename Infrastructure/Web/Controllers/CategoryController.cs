using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<CategoryDto>>.ErrorResult($"Error retrieving categories: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);
                if (result.Success && result.Data != null)
                {
                    return Ok(result);
                }
                return NotFound(BaseResponseDto<CategoryDto>.ErrorResult("Category not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<CategoryDto>.ErrorResult($"Error retrieving category: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                var result = await _categoryService.CreateCategoryAsync(createCategoryDto);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetCategory), new { id = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<CategoryDto>.ErrorResult($"Error creating category: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseDto<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<CategoryDto>.ErrorResult($"Error updating category: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error deleting category: {ex.Message}"));
            }
        }
    }
}
