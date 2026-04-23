using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.MultiTenancy;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Planning;
using WMSSolution.WMS.IServices.Planning;

namespace WMSSolution.WMS.Services.Planning;

/// <summary>
/// Planning Service
/// </summary>
public class PlanningService(SqlDBContext dbContext,
    IConfiguration configuration,
    IStringLocalizer<MultiLanguage> stringLocalizer) : IPlanningService
{
    private readonly IConfiguration _configuration = configuration;
    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext = dbContext;
    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Get Picking List
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<PickingDTO>> GetPickingList(CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        var query = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId);
        var queryDetail = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>();
        var skuList = _dbContext.GetDbSet<SkuEntity>();
        var unitList = _dbContext.GetDbSet<SkuUomEntity>();
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);

        var processingStatus = new List<ReceiptStatus>
        {
            ReceiptStatus.PROCESSING,
            ReceiptStatus.NEW
        };

        query = query.Where(x => processingStatus.Contains(x.Status));

        var hasShipDate = query.Where(x => x.ExpectedShipDate == null);
        var allowShipDate = query.Where(x => x.ExpectedShipDate != null);
        var queryPicking = _dbContext.GetDbSet<PickingEntity>(tenantId);

        var pickingIds = await queryPicking.Select(x => x.PickingId).ToListAsync();

        var data = await hasShipDate.Select(x => new PickingDTO
        {
            ReceiptId = x.Id,
            ReceiptNo = x.ReceiptNumber,
            WarehouseId = x.WarehouseId,
            ExpectedShipDate = x.ExpectedShipDate,
            StartPickingTime = x.StartPickingTime,
            GatewayId = x.OutboundGatewayId
        })
            .ToListAsync();

        var currentDate = DateTime.UtcNow;
        var forecasting = await allowShipDate
            .Where(x => x.ExpectedShipDate >= currentDate 
                || x.ReceiptDate == null
                || x.ReceiptDate < currentDate)
            .Select(x => new PickingDTO
            {
                ReceiptId = x.Id,
                ReceiptNo = x.ReceiptNumber,
                WarehouseId = x.WarehouseId,
                ExpectedShipDate = x.ExpectedShipDate,
                StartPickingTime = x.StartPickingTime,
                GatewayId = x.OutboundGatewayId
            })
            .ToListAsync();

        var results = new List<PickingDTO>();
        foreach (var item in data.Union(forecasting))  
        {
            var warehouse = await warehouseList
                .FirstOrDefaultAsync(x => x.Id == item.WarehouseId);

            var receiptDetails = await queryDetail
                .Where(x => x.ReceiptId == item.ReceiptId)
                .Where(x => !pickingIds.Any(id => x.Id == id))
                .Select(x => new PickingDTO
                {                    
                    Id = x.Id,
                    ReceiptId = x.ReceiptId,
                    ExpectedShipDate = item.ExpectedShipDate,
                    StartPickingTime = item.StartPickingTime,
                    LocationId = x.LocationId,
                    ReceiptNo = item.ReceiptNo,
                    SkuId = x.SkuId,
                    SkuUomId = x.SkuUomId,
                    WarehouseId = item.WarehouseId,
                    Quantity = x.Quantity,   
                    GatewayId = item.GatewayId
                })
                .ToListAsync();

            foreach (var picking in receiptDetails)
            {                
                var unit = unitList.FirstOrDefault(x => x.Id == picking.SkuUomId);
                var location = locationList.FirstOrDefault(x => x.Id == picking.LocationId);
                var sku = skuList.FirstOrDefault(x => x.Id == picking.SkuId);
                var locationName = location?.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation 
                    ? "Virtual" : location?.LocationName ?? "";

                picking.IsVirtualLocation = location?.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation;
                picking.SkuCode = sku?.sku_code ?? "";
                picking.SkuName = sku?.sku_name ?? "";
                picking.LocationName = locationName;
                picking.UnitName = unit?.UnitName ?? "";
                picking.WarehouseName = warehouse?.WarehouseName ?? "";
                picking.WarehouseAddress = warehouse?.address ?? "";

                results.Add(picking);
            }
        }

        return results;
    }

    /// <summary>
    /// Planning Packing
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<PickingDTO>> GetPlanningPackingList(CurrentUser currentUser)
    {
        var tenantId = currentUser.tenant_id;
        var queryPicking = _dbContext.GetDbSet<PickingEntity>(tenantId);
        var results = new List<PickingDTO>();
        var items = await queryPicking.ToListAsync();
        var skuList = _dbContext.GetDbSet<SkuEntity>();
        var unitList = _dbContext.GetDbSet<SkuUomEntity>();
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId);

        foreach (var item in items)
        {
            var warehouse = await warehouseList
               .FirstOrDefaultAsync(x => x.Id == item.WarehouseId);

            var unit = unitList.FirstOrDefault(x => x.Id == item.SkuUomId);
            var location = locationList.FirstOrDefault(x => x.Id == item.LocationId);
            var sku = skuList.FirstOrDefault(x => x.Id == item.SkuId);
            var locationName = location?.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation
                ? "Virtual" : location?.LocationName ?? "";

            var outbound = await outboundList.FirstOrDefaultAsync(x => x.Id == item.ReceiptId);

            results.Add(new PickingDTO
            {
                Id = item.PickingId,
                ReceiptNo = outbound?.ReceiptNumber ?? "",
                ReceiptId = item.ReceiptId,
                WarehouseId = item.WarehouseId ?? 0,
                SkuId = item.SkuId,
                Quantity = item.Quantity,
                SkuUomId = item.SkuUomId,
                LocationId = item.LocationId,
                PalletCode = item.PalletCode,
                ExpiryDate = item.ExpiryDate,
                GatewayId = item.GatewayId,
                IsVirtualLocation = location?.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation,
                SkuCode = sku?.sku_code ?? "",
                SkuName = sku?.sku_name ?? "",
                LocationName = locationName,
                UnitName = unit?.UnitName ?? "",
                ExpectedShipDate = outbound?.ExpectedShipDate,
                WarehouseName = warehouse?.WarehouseName ?? "",
                WarehouseAddress = warehouse?.address ?? ""
            });
        }

        return results;
    }

    /// <summary>
    /// Save Picking List
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool Success, string? Message)> SavePickingList(IEnumerable<PickingDTO> requests, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var query = _dbContext.GetDbSet<PickingEntity>(tenantId);

        try
        {
            foreach (var request in requests)
            {
                var entity = await query
                    .FirstOrDefaultAsync(x => x.PickingId == request.Id, cancellationToken);
                if (entity == null)
                {
                    _dbContext.GetDbSet<PickingEntity>().Add(new PickingEntity
                    {
                        PickingId = request.Id,                       
                        ReceiptId = request.ReceiptId,
                        WarehouseId = request.WarehouseId,
                        SkuId = request.SkuId,
                        Quantity = request.Quantity,
                        SkuUomId = request.SkuUomId,
                        CreateDate = DateTime.UtcNow,
                        LocationId = request.LocationId,
                        PalletCode = request.PalletCode,
                        ExpiryDate = request.ExpiryDate,
                        GatewayId = request.GatewayId,
                        TenantId = tenantId                       
                    });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
