using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;
using WMSSolution.WMS.IServices.Receipt;

namespace WMSSolution.WMS.Services.Receipt;

/// <summary>
/// Inbound pallet service
/// </summary>
public class InboundPalletService(
    SqlDBContext dbContext,
    IStringLocalizer<MultiLanguage> localizer,
    FunctionHelper functionHelper,
    ILogger<InboundPalletService> logger) : BaseService<InboundPallet>, IInboundPalletService
{
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly IStringLocalizer<MultiLanguage> _localizer = localizer;
    private readonly FunctionHelper _functionHelper = functionHelper;
    private readonly ILogger<InboundPalletService> _logger = logger;

    /// <inheritdoc />
    public async Task<(int id, string message)> CreateAsync(
        CreateInboundPalletRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var validation = ValidateCreateRequest(request);
        if (!validation.isValid)
        {
            return (0, validation.message);
        }

        var inboundPalletDbSet = _dbContext.GetDbSet<InboundPallet>(currentUser.tenant_id, true);
        if (string.IsNullOrWhiteSpace(request.PalletCode))
        {
            request.PalletCode = await _functionHelper.GetFormNoAsync("pallet", "PLT");
        }

        var isDuplicateCode = await inboundPalletDbSet
            .AnyAsync(x => x.PalletCode == request.PalletCode, cancellationToken);

        if (isDuplicateCode)
        {
            return (0, _localizer["Pallet code already exists"]);
        }


        var entity = new InboundPallet
        {
            PalletCode = request.PalletCode,
            PalletRFID = request.PalletRFID?.Trim(),
            LocationId = request.LocationId,
            Description = request.Description?.Trim(),
            CreatedTime = DateTime.UtcNow,
            LastUpdatedTime = DateTime.UtcNow,
            TenantId = currentUser.tenant_id,
            IsMixed = request.IsMixed
        };

        foreach (var item in request.Details)
        {
            entity.Details.Add(new InboundPalletDetail
            {
                SkuId = item.SkuId,
                SkuUomId = item.SkuUomId,
                Quantity = item.Quantity,
                SupplierId = item.SupplierId,
                ExpiryDate = item.ExpiryDate
            });
        }

        await _dbContext.GetDbSet<InboundPallet>().AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);


        return (entity.Id, _localizer["save_success"]);
    }

    private (bool isValid, string message) ValidateCreateRequest(CreateInboundPalletRequest request)
    {
        if (request == null)
        {
            return (false, _localizer["Invalid request"]);
        }

        if (request.LocationId <= 0)
        {
            return (false, _localizer["Location is required"]);
        }

        if (request.Details == null || request.Details.Count == 0)
        {
            return (false, _localizer["Details are required"]);
        }

        if (request.Details.Any(x => x.SkuId <= 0 || x.SkuUomId <= 0 || x.Quantity <= 0))
        {
            return (false, _localizer["Invalid detail item"]);
        }

        var distinctSkuIds = request.Details.Select(x => x.SkuId).Distinct().ToList();
        if (!request.IsMixed && distinctSkuIds.Count > 1)
        {
            _logger.LogWarning("Non-mixed pallet contains multiple SKUs. PalletCode: {PalletCode}, DistinctSkuCount: {DistinctSkuCount}",
                request.PalletCode, distinctSkuIds.Count);
            return (false, _localizer["Non-mixed pallet can only contain one SKU"]);
        }

        return (true, string.Empty);
    }
}
