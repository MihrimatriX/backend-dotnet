using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : ApiControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<CategoryDto>>>> GetCategories() =>
            Respond(await _categoryService.GetAllCategoriesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<CategoryDto>>> GetCategory(int id) =>
            Respond(await _categoryService.GetCategoryByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto) =>
            RespondCreated(await _categoryService.CreateCategoryAsync(createCategoryDto));

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto) =>
            Respond(await _categoryService.UpdateCategoryAsync(id, updateCategoryDto));

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteCategory(int id) =>
            Respond(await _categoryService.DeleteCategoryAsync(id));
    }
}
