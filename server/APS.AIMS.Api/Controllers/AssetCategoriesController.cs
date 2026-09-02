using APS.AIMS.Application.AssetCategories;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/asset-categories")]
public sealed class AssetCategoriesController(
    IAssetCategoryService assetCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetCategoryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await assetCategoryService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetCategoryDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await assetCategoryService.GetByIdAsync(
            id,
            cancellationToken);

        return category is null ? NotFound() : Ok(category);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPost]
    public async Task<ActionResult<AssetCategoryDto>> Create(
        CreateAssetCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await assetCategoryService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            category);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetCategoryDto>> Update(
        Guid id,
        UpdateAssetCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await assetCategoryService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return category is null ? NotFound() : Ok(category);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await assetCategoryService.DeactivateAsync(
            id,
            cancellationToken);

        return result ? NoContent() : NotFound();
    }
}
