using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubCategoryController : ControllerBase
{
    private readonly ISubCategoryService _subCategoryService;

    public SubCategoryController(ISubCategoryService subCategoryService)
    {
        _subCategoryService = subCategoryService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponseDto<IEnumerable<SubCategoryDto>>>> GetAllSubCategories()
    {
        try
        {
            var subCategories = await _subCategoryService.GetAllSubCategoriesAsync();
            return Ok(new BaseResponseDto<IEnumerable<SubCategoryDto>>
            {
                Success = true,
                Data = subCategories,
                Message = "SubCategories retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponseDto<IEnumerable<SubCategoryDto>>
            {
                Success = false,
                Data = Enumerable.Empty<SubCategoryDto>(),
                Message = $"Error retrieving subcategories: {ex.Message}"
            });
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<BaseResponseDto<IEnumerable<SubCategoryDto>>>> GetSubCategoriesByCategoryId(int categoryId)
    {
        try
        {
            var subCategories = await _subCategoryService.GetSubCategoriesByCategoryIdAsync(categoryId);
            return Ok(new BaseResponseDto<IEnumerable<SubCategoryDto>>
            {
                Success = true,
                Data = subCategories,
                Message = "SubCategories retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponseDto<IEnumerable<SubCategoryDto>>
            {
                Success = false,
                Data = Enumerable.Empty<SubCategoryDto>(),
                Message = $"Error retrieving subcategories: {ex.Message}"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponseDto<SubCategoryDto>>> GetSubCategoryById(int id)
    {
        try
        {
            var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
            {
                return NotFound(new BaseResponseDto<SubCategoryDto>
                {
                    Success = false,
                    Data = null!,
                    Message = "SubCategory not found"
                });
            }

            return Ok(new BaseResponseDto<SubCategoryDto>
            {
                Success = true,
                Data = subCategory,
                Message = "SubCategory retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponseDto<SubCategoryDto>
            {
                Success = false,
                Data = null!,
                Message = $"Error retrieving subcategory: {ex.Message}"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponseDto<SubCategoryDto>>> CreateSubCategory([FromBody] CreateSubCategoryDto createSubCategoryDto)
    {
        try
        {
            var subCategory = await _subCategoryService.CreateSubCategoryAsync(createSubCategoryDto);
            return CreatedAtAction(nameof(GetSubCategoryById), new { id = subCategory.Id }, new BaseResponseDto<SubCategoryDto>
            {
                Success = true,
                Data = subCategory,
                Message = "SubCategory created successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponseDto<SubCategoryDto>
            {
                Success = false,
                Data = null!,
                Message = $"Error creating subcategory: {ex.Message}"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponseDto<SubCategoryDto>>> UpdateSubCategory(int id, [FromBody] UpdateSubCategoryDto updateSubCategoryDto)
    {
        try
        {
            if (id != updateSubCategoryDto.Id)
            {
                return BadRequest(new BaseResponseDto<SubCategoryDto>
                {
                    Success = false,
                    Data = null!,
                    Message = "ID mismatch"
                });
            }

            var subCategory = await _subCategoryService.UpdateSubCategoryAsync(updateSubCategoryDto);
            return Ok(new BaseResponseDto<SubCategoryDto>
            {
                Success = true,
                Data = subCategory,
                Message = "SubCategory updated successfully"
            });
        }
        catch (ArgumentException)
        {
            return NotFound(new BaseResponseDto<SubCategoryDto>
            {
                Success = false,
                Data = null!,
                Message = "SubCategory not found"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponseDto<SubCategoryDto>
            {
                Success = false,
                Data = null!,
                Message = $"Error updating subcategory: {ex.Message}"
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponseDto<bool>>> DeleteSubCategory(int id)
    {
        try
        {
            var result = await _subCategoryService.DeleteSubCategoryAsync(id);
            if (!result)
            {
                return NotFound(new BaseResponseDto<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "SubCategory not found"
                });
            }

            return Ok(new BaseResponseDto<bool>
            {
                Success = true,
                Data = true,
                Message = "SubCategory deleted successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponseDto<bool>
            {
                Success = false,
                Data = false,
                Message = $"Error deleting subcategory: {ex.Message}"
            });
        }
    }
}
