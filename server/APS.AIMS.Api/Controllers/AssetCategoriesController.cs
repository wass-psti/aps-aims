using APS.AIMS.Application.AssetCategories;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/asset-categories")]
public class AssetCategoriesController : ControllerBase
{
    private readonly IAssetCategoryService _assetCategoryService;

    public AssetCategoriesController(
        IAssetCategoryService assetCategoryService)
    {
        _assetCategoryService = assetCategoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetCategoryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _assetCategoryService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetCategoryDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category =
            await _assetCategoryService.GetByIdAsync(
                id,
                cancellationToken);

        return category is null
            ? NotFound()
            : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<AssetCategoryDto>> Create(
        CreateAssetCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category =
                await _assetCategoryService.CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetCategoryDto>> Update(
        Guid id,
        UpdateAssetCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category =
                await _assetCategoryService.UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            return category is null
                ? NotFound()
                : Ok(category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _assetCategoryService.DeactivateAsync(
                    id,
                    cancellationToken);

            return result
                ? NoContent()
                : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}