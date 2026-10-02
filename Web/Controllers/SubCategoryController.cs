using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Infrastructure.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Web.Controllers;

[Route("api/[controller]")]
public class SubCategoryController : ApiControllerBase
{
    private readonly ISubCategoryService _subCategoryService;

    public SubCategoryController(ISubCategoryService subCategoryService)
    {
        _subCategoryService = subCategoryService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponseDto<List<SubCategoryDto>>>> GetAllSubCategories() =>
        Respond(await _subCategoryService.GetAllSubCategoriesAsync());

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<BaseResponseDto<List<SubCategoryDto>>>> GetSubCategoriesByCategoryId(int categoryId) =>
        Respond(await _subCategoryService.GetSubCategoriesByCategoryIdAsync(categoryId));

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponseDto<SubCategoryDto>>> GetSubCategoryById(int id) =>
        Respond(await _subCategoryService.GetSubCategoryByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<BaseResponseDto<SubCategoryDto>>> CreateSubCategory([FromBody] CreateSubCategoryDto createSubCategoryDto) =>
        RespondCreated(await _subCategoryService.CreateSubCategoryAsync(createSubCategoryDto));

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<BaseResponseDto<SubCategoryDto>>> UpdateSubCategory(int id, [FromBody] UpdateSubCategoryDto updateSubCategoryDto) =>
        Respond(await _subCategoryService.UpdateSubCategoryAsync(id, updateSubCategoryDto));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<BaseResponseDto<bool>>> DeleteSubCategory(int id) =>
        Respond(await _subCategoryService.DeleteSubCategoryAsync(id));
}
