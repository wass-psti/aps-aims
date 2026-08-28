using System.Linq.Expressions;
using APS.AIMS.Application.Assets;
using APS.AIMS.Domain.Entities;

namespace APS.AIMS.Infrastructure.Services;

internal static class AssetProjection
{
    public static readonly Expression<Func<Asset, AssetDto>> ToDto =
        asset => new AssetDto
        {
            Id = asset.Id,
            AssetId = asset.AssetId,
            BarcodeValue = asset.BarcodeValue,
            Name = asset.Name,
            ShortDescription = asset.ShortDescription,

            CategoryId = asset.CategoryId,
            CategoryName = asset.Category.Name,

            SerialNumber = asset.SerialNumber,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            PartNumber = asset.PartNumber,
            LegacyAssetId = asset.LegacyAssetId,

            AcquisitionCost = asset.AcquisitionCost,
            Currency = asset.Currency,

            CompanyId = asset.CompanyId,
            CompanyName = asset.Company.Name,

            BranchId = asset.BranchId,
            BranchName = asset.Branch.Name,

            DepartmentId = asset.DepartmentId,
            DepartmentName = asset.Department != null
                ? asset.Department.Name
                : null,

            CurrentLocationId = asset.CurrentLocationId,
            CurrentLocationName = asset.CurrentLocation.Name,

            CurrentCustodianId = asset.CurrentCustodianId,
            CurrentCustodianName = asset.CurrentCustodian != null
                ? asset.CurrentCustodian.FirstName + " " +
                  asset.CurrentCustodian.LastName
                : null,

            Status = asset.Status,
            Condition = asset.Condition,

            IsArchived = asset.IsArchived,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
}