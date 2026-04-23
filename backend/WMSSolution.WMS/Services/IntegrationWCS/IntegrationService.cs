using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Core.Utility;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.OutboundGateway;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Swap;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;
using WMSSolution.WMS.Entities.ViewModels.Stock;
using WMSSolution.WMS.Entities.ViewModels.StockTransaction;
using WMSSolution.WMS.Entities.ViewModels.Warehouse;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.IntegrationWCS;
using WMSSolution.WMS.IServices.Stock;
using WMSSolution.WMS.IServices.Warehouse;
using WMSSolution.WMS.Services.Receipt;

namespace WMSSolution.WMS.Services.IntegrationWCS;

/// <summary>
/// Integration with WCS
/// </summary>
public class IntegrationService(SqlDBContext dbContext,
    IHttpClientFactory httpClientFactory,
    IActionLogService actionLogService,
    IStringLocalizer<MultiLanguage> localizer,
    ILogger<IntegrationService> logger,
    IServiceProvider serviceProvider,
    FunctionHelper functionHelpers,
    IStockService stockService,
    IWarehouseService warehouseService) : IIntegrationService
{
    /// <summary>
    /// DB Context
    /// </summary>
    private readonly SqlDBContext _dbContext = dbContext;

    /// <summary>
    /// Action log service
    /// </summary>
    private readonly IActionLogService _actionLogService = actionLogService;

    /// <summary>
    /// Stock Service
    /// </summary>
    private readonly IStockService _stockService = stockService;

    /// <summary>
    /// Warehouse Service
    /// </summary>
    private readonly IWarehouseService _warehouseService = warehouseService;

    /// <summary>
    /// 
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _localizer = localizer;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<IntegrationService> _logger = logger;

    /// <summary>
    ///  service Provider
    /// </summary>
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// function helper
    /// </summary>
    private readonly FunctionHelper _functionHelper = functionHelpers;

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Asynchronously retrieves a collection of inbound task records.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<IEnumerable<InboundTaskResponse>> GetInboundTaskAsync(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        // only get the ready status inbound
        var inbounds = await _dbContext.Inbounds
            .Where(x => x.Status == IntegrationStatus.Ready && x.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (inbounds.Count == 0) return [];

        var inboundIds = inbounds.Select(x => x.Id).ToList();
        var locationIds = inbounds.Select(x => x.LocationId).Distinct().ToList();

        var locations = await _dbContext.GetDbSet<GoodslocationEntity>()
                .AsNoTracking()
                .Where(l => locationIds.Contains(l.Id))
                .ToListAsync(cancellationToken);

        var warehouseIds = locations.Select(l => l.WarehouseId).Distinct().ToList();

        await _actionLogService.AddLogAsync(
            $"[Inbound Task] Get tasks with number {inbounds.Count} rows, warehouseIds={string.Join(",", warehouseIds)}",
            "Integration", currentUser);

        var warehouses = await _dbContext.GetDbSet<WarehouseEntity>(currentUser.tenant_id)
            .Where(w => warehouseIds.Contains(w.Id))
            .ToListAsync(cancellationToken);

        //var areaIds = locations.Where(l => l.WarehouseAreaId > 0)
        //                       .Select(l => l.WarehouseAreaId)
        //                       .Distinct()
        //                       .ToList();

        //var areas = await _dbContext.GetDbSet<WarehouseareaEntity>()
        //    .AsNoTracking()
        //    .Where(a => areaIds.Contains(a.Id))
        //    .ToListAsync();

        var mappings = await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
                                     .AsNoTracking()
                                     .Where(m => inboundIds.Contains(m.InboundId))
                                     .ToListAsync(cancellationToken);

        var receiptDetailIds = mappings.Select(m => m.ReceiptDetailId).Distinct().ToList();

        var receiptDetails = await _dbContext.GetDbSet<InboundReceiptDetailEntity>()
                                             .AsNoTracking()
                                             .Where(r => receiptDetailIds.Contains(r.Id))
                                             .ToListAsync(cancellationToken);

        var receiptIds = receiptDetails.Select(r => r.ReceiptId).Distinct().ToList();
        var skuIds = receiptDetails.Select(r => r.SkuId).Distinct().ToList();

        var skus = await _dbContext.GetDbSet<SkuEntity>()
                                   .AsNoTracking()
                                   .Where(s => skuIds.Contains(s.Id))
                                   .ToListAsync(cancellationToken);

        var receipts = await _dbContext.GetDbSet<InboundReceiptEntity>()
                                   .AsNoTracking()
                                   .Where(r => receiptIds.Contains(r.Id))
                                   .ToListAsync(cancellationToken);

        var supplierIds = receipts.Select(r => r.SupplierId).Distinct().ToList();
        var suppliers = await _dbContext.GetDbSet<SupplierEntity>()
                                         .AsNoTracking()
                                         .Where(s => supplierIds.Contains(s.Id))
                                         .ToListAsync(cancellationToken);


        var inboundFullInfos = (from inbound in inbounds
                                join map in mappings on inbound.Id equals map.InboundId
                                join detail in receiptDetails on map.ReceiptDetailId equals detail.Id
                                join sku in skus on detail.SkuId equals sku.Id into skuGroup
                                from sku in skuGroup.DefaultIfEmpty()
                                join receipt in receipts on detail.ReceiptId equals receipt.Id into receiptGroup
                                from receipt in receiptGroup.DefaultIfEmpty()
                                join supplier in suppliers on receipt?.SupplierId equals supplier.Id into supplierGroup
                                from supplier in supplierGroup.DefaultIfEmpty()

                                select new
                                {
                                    InboundId = inbound.Id,
                                    TaskCode = inbound.TaskCode,
                                    SkuCode = sku?.sku_code ?? "",
                                    ReceiptNumber = receipt?.ReceiptNumber ?? "",
                                    SupplierName = supplier?.supplier_name ?? ""
                                }).ToList();

        var result = inbounds
         .GroupBy(x => x.TaskCode)
         .Select(g =>
         {
             var firstItem = g.First();
             var headerInfo = inboundFullInfos.FirstOrDefault(info => info.InboundId == firstItem.Id);

             return new InboundTaskResponse
             {
                 TaskCode = firstItem.TaskCode,
                 PickDate = firstItem.PickUpDate.GetValueOrDefault(),
                 ResponseDate = DateTime.UtcNow,
                 AsnNumber = headerInfo?.ReceiptNumber ?? string.Empty,
                 GoodOwnerName = headerInfo?.SupplierName ?? string.Empty,
                 SkuCode = headerInfo?.SkuCode ?? string.Empty,
                 Details = [.. g.Select(x =>
                {
                    var location = locations.FirstOrDefault(l => l.Id == x.LocationId);
                  //  var area = areas.FirstOrDefault(a => a.Id == location?.WarehouseAreaId);
                    
                    var warehouse = warehouses.FirstOrDefault(w => w.Id == location?.WarehouseId);
                    var blockId = warehouse?.WcsBlockId;
                    return new InboundTaskDTO
                    {
                        PalletCode = x.PalletCode,
                        Status = x.Status.ToString(),
                        Location = location != null
                                ? $"{location.CoordinateZ}.{location.CoordinateX}.{location.CoordinateY}"
                                : string.Empty,
                        BlockId = blockId ?? string.Empty,
                    };
                })]
             };
         })
         .ToList();

        return result;
    }

    /// <summary>
    /// Asynchronously retrieves a collection of outbound tasks.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<IEnumerable<OutboundTaskResponse>> GetOutboundTaskAsync(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var outbounds = await _dbContext.Outbounds
                               .Where(x => x.Status == IntegrationStatus.Ready)
                               .AsNoTracking()
                               .ToListAsync(cancellationToken);

        if (outbounds.Count == 0)
        {
            return [];
        }

        var outboundIds = outbounds.Select(x => x.Id).ToList();
        var locationIds = outbounds.Select(x => x.LocationId).Distinct().ToList();

        var locations = await _dbContext.GetDbSet<GoodslocationEntity>()
           .AsNoTracking()
           .Where(l => locationIds.Contains(l.Id))
           .ToListAsync(cancellationToken: cancellationToken);

        var warehouseIds = locations.Select(l => l.WarehouseId).Distinct().ToList();

        await _actionLogService.AddLogAsync(
            $"[Outbound Task] Get tasks with number {outbounds.Count} rows, warehouseIds={string.Join(",", warehouseIds)}",
            "Integration", currentUser);

        var warehouses = await _dbContext.GetDbSet<WarehouseEntity>(currentUser.tenant_id)
            .Where(w => warehouseIds.Contains(w.Id))
            .ToListAsync(cancellationToken);

        var integrations = await _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
          .AsNoTracking()
          .Where(i => outboundIds.Contains(i.OutboundId))
          .ToListAsync(cancellationToken);

        var detailIds = integrations.Select(i => i.ReceiptDetailId).Distinct().ToList();

        var receiptDetails = await _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
        .AsNoTracking()
        .Where(d => detailIds.Contains(d.Id))
        .ToListAsync(cancellationToken: cancellationToken);

        var skuIds = receiptDetails.Select(d => d.SkuId).Distinct().ToList();
        var receiptIds = receiptDetails.Select(d => d.ReceiptId).Distinct().ToList();

        // 5. Fetch SKU
        var skus = await _dbContext.GetDbSet<SkuEntity>(currentUser.tenant_id)
            .Where(s => skuIds.Contains(s.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var receipts = await _dbContext.GetDbSet<OutBoundReceiptEntity>(currentUser.tenant_id)
         .Where(r => receiptIds.Contains(r.Id))
         .ToListAsync(cancellationToken: cancellationToken);

        var customerIds = receipts.Select(r => r.CustomerId).Distinct().ToList();
        var customers = await _dbContext.GetDbSet<CustomerEntity>(currentUser.tenant_id)
            .Where(c => customerIds.Contains(c.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var gatewayIds = receipts.Select(r => r.OutboundGatewayId).Distinct().ToList();
        var gateways = await _dbContext.GetDbSet<OutboundGatewayEntity>()
            .AsNoTracking()
            .Where(g => gatewayIds.Contains(g.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var outboundFullInfos = (from i in integrations
                                 join d in receiptDetails on i.ReceiptDetailId equals d.Id
                                 join r in receipts on d.ReceiptId equals r.Id
                                 join c in customers on r.CustomerId equals c.Id into cg
                                 from c in cg.DefaultIfEmpty()
                                 join s in skus on d.SkuId equals s.Id into sg
                                 from s in sg.DefaultIfEmpty()
                                 join gw in gateways on r.OutboundGatewayId equals gw.Id into gwg
                                 from gw in gwg.DefaultIfEmpty()
                                 select new
                                 {
                                     OutboundId = i.OutboundId,
                                     SoNumber = r.ReceiptNumber,
                                     CustomerName = c?.customer_name ?? string.Empty,
                                     SkuCode = s?.sku_code ?? string.Empty,
                                     GatewayName = gw?.GatewayName ?? string.Empty,
                                 }).ToList();

        var result = outbounds
        .GroupBy(x => x.TaskCode)
        .Select(g =>
        {
            var firstItem = g.First();
            var info = outboundFullInfos.FirstOrDefault(p => p.OutboundId == firstItem.Id);
            return new OutboundTaskResponse
            {
                TaskCode = firstItem.TaskCode,
                PickDate = firstItem.PickUpDate.GetValueOrDefault(),
                ResponseDate = DateTime.UtcNow,
                SoNumber = info?.SoNumber ?? string.Empty,
                GatewayName = info?.GatewayName ?? string.Empty,
                CustomerName = info?.CustomerName ?? string.Empty,
                SkuCode = info?.SkuCode ?? string.Empty,
                Details = [.. g.Select(x =>
                {
                    var location = locations.FirstOrDefault(l => l.Id == x.LocationId);
                    var warehouse = warehouses.FirstOrDefault(w => w.Id == location?.WarehouseId);
                    var blockId = warehouse?.WcsBlockId;

                    return new OutboundTaskDTO
                    {
                        PalletCode = x.PalletCode,
                        Status = x.Status.ToString(),
                        Location = location != null
                                ? $"{location.CoordinateZ}.{location.CoordinateX}.{location.CoordinateY}"
                                : string.Empty,
                        BlockId = blockId ?? string.Empty,
                    };
                })]
            };
        }).ToList();

        return result;
    }

    /// <summary>
    /// Reshuffling
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<SwapPalletDTO>> ReshufflingAsync(CurrentUser currentUser)
    {
        var data = await _dbContext.SwapPallets
            .Where(x => x.Status != IntegrationStatus.Done)
            .ToListAsync();

        await _actionLogService.AddLogAsync(
            $"[Swap Pallets] Get tasks with number {data.Count} rows",
            "Integration", currentUser);

        var locations = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();
        return data.Select(x =>
        {
            var location = locations.FirstOrDefault(l => l.Id == x.FromLocationId);
            var toLocation = locations.FirstOrDefault(l => l.Id == x.ToLocationId);
            string statusName = Enum.GetName(typeof(IntegrationStatus), x.Status) ?? "";

            return new SwapPalletDTO
            {
                SwapId = x.Id,
                PalletCode = x.PalletCode,
                Status = statusName,
                FromLocation = $"{location?.CoordinateZ}.{location?.CoordinateX}.{location?.CoordinateY}",
                ToLocation = $"{toLocation?.CoordinateZ}.{toLocation?.CoordinateX}.{toLocation?.CoordinateY}"
            };
        });
    }

    /// <summary>
    /// Update Pallet Location
    /// </summary>
    /// <param name="palletCode"></param>
    /// <param name="location"></param>
    /// <param name="toLocation"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> UpdatePalletLocationAsync(string palletCode, string location, string toLocation, CurrentUser currentUser)
    {
        var locations = _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking();

        var fromLoc = locations.FirstOrDefault(l =>
            $"{l.CoordinateZ}.{l.CoordinateX}.{l.CoordinateY}" == location);
        var toLoc = locations.FirstOrDefault(l =>
            $"{l.CoordinateZ}.{l.CoordinateX}.{l.CoordinateY}" == toLocation);

        await _actionLogService.AddLogAsync(
            $"[Update Pallet Location] palletCode {palletCode} from {location} to {toLocation}",
            "Integration", currentUser);

        _dbContext.IntegrationHistories.Add(new IntegrationHistory
        {
            Status = IntegrationStatus.Done,
            CreatedDate = DateTime.UtcNow,
            LocationId = toLoc?.Id ?? 0,
            HistoryType = HistoryType.Swap,
            PalletCode = palletCode,
            Priority = 1,
            IsActive = true,
            PickUpDate = DateTime.UtcNow
        });

        //TODO update new location for pallet in your system
        return await _dbContext.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Update Reshuffling
    /// </summary>
    /// <param name="swapId"></param>
    /// <param name="status"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> UpdateReshufflingAsync(long swapId, IntegrationStatus status, CurrentUser currentUser)
    {
        var data = await _dbContext.SwapPallets
            .Where(x => x.Status != IntegrationStatus.Done && x.Id == swapId)
            .FirstOrDefaultAsync();

        if (data != null)
        {
            await _actionLogService.AddLogAsync(
                $"[Update Reshuffling] SwapId {swapId} Status {status} SwapPallet {data.Id} PalletCode={data.PalletCode} ToLocationId={data.ToLocationId}",
                "Integration", currentUser);

            data.Status = status;
            _dbContext.SwapPallets.Update(data);

            _dbContext.IntegrationHistories.Add(new IntegrationHistory
            {
                Status = status,
                CreatedDate = DateTime.UtcNow,
                LocationId = data.ToLocationId,
                HistoryType = HistoryType.RequestSwap,
                PalletCode = data.PalletCode,
                Priority = data.Priority,
                IsActive = true,
                PickUpDate = DateTime.UtcNow,
                TenantId = data.TenantId
            });
        }

        return await _dbContext.SaveChangesAsync() > 0;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="isInbound"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> SaveDataIntegration(bool isInbound = true)
    {
        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            Status = IntegrationStatus.Done,
            CreatedDate = DateTime.UtcNow,
            LocationId = 1,
            HistoryType = isInbound ? HistoryType.Inbound : HistoryType.Outbound,
            PalletCode = "PalletCode",
            Priority = 1,
            IsActive = true,
            PickUpDate = DateTime.UtcNow
        });
        return await _dbContext.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Creates InboundEntity records from DTOs.
    /// This method is responsible only for creating and persisting Inbound entities.
    /// </summary>
    /// <param name="tasks">List of inbound task DTOs</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>List of created InboundEntity objects with assigned IDs</returns>
    public async Task<List<InboundEntity>> CreateInboundEntitiesAsync(
        List<CreateInboundTaskDTO> tasks,
        CurrentUser currentUser)
    {
        if (tasks == null || tasks.Count == 0)
        {
            return [];
        }

        // handle multiple palletcode with the same task code
        var shareTaskCode = GenarationHelper.GenerateTaskCode();
        var inboundEntities = tasks.Select(dto => new InboundEntity
        {
            TenantId = currentUser.tenant_id,
            PalletCode = dto.PalletCode,
            TaskCode = shareTaskCode,
            LocationId = dto.LocationId,
            CreatedDate = DateTime.UtcNow,
            PickUpDate = dto.PickUpDate,
            Priority = dto.Priority,
            Status = IntegrationStatus.Processing,
            IsActive = dto.IsActive,
        }).ToList();

        await _dbContext.Inbounds.AddRangeAsync(inboundEntities);
        var result = await _dbContext.SaveChangesAsync();

        if (result <= 0)
        {
            _logger.LogError("Failed to create inbound entities for User: {UserId}", currentUser.user_id);
            return [];
        }

        await _actionLogService.AddLogAsync(
            $"[Create Inbound Entities] Receipts {inboundEntities.Count} rows, PalletCodes={string.Join(";", tasks.Select(x => x.PalletCode))} => {string.Join(";", inboundEntities.Select(x => x.PalletCode))}",
            "Integration", currentUser);

        return inboundEntities;
    }

    /// <summary>
    /// Creates IntegrationHistory records from InboundEntity list.
    /// This method is responsible only for creating history records for audit/tracking purposes.
    /// </summary>
    /// <param name="inboundEntities">List of inbound entities to create history for</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>True if history records were created successfully</returns>
    public async Task<bool> CreateIntegrationHistoryAsync(
        List<InboundEntity> inboundEntities,
        CurrentUser currentUser)
    {
        if (inboundEntities == null || inboundEntities.Count == 0)
        {
            return false;
        }

        var historyEntities = inboundEntities.Select(inbound => new IntegrationHistory
        {
            Status = IntegrationStatus.Processing,
            CreatedDate = DateTime.UtcNow,
            LocationId = inbound.LocationId,
            HistoryType = HistoryType.Inbound,
            PalletCode = inbound.PalletCode,
            Priority = inbound.Priority,
            IsActive = true,
            PickUpDate = inbound.PickUpDate,
            TaskCode = inbound.TaskCode,
            TenantId = currentUser.tenant_id,
        }).ToList();

        await _dbContext.IntegrationHistories.AddRangeAsync(historyEntities);
        var result = await _dbContext.SaveChangesAsync();

        return result > 0;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> UpdateInboundSuccessStatusAsync(InboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (request == null)
        {
            _logger.LogWarning("UpdateStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Inbounds
            .FirstOrDefaultAsync(x => x.TaskCode.Trim() == request.TaskCode.Trim()
                                   && x.PalletCode.Trim() == request.PalletCode.Trim()
                                   && x.Status != IntegrationStatus.Done && x.IsActive, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Inbound task not found or not processing: {PalletCode}", request.PalletCode);
            return false;
        }

        if (!string.IsNullOrEmpty(key))
        {
            if (key != task.WcsKey)
            {
                _logger.LogWarning("WCS Key mismatch for TaskCode: {TaskCode}. Expected: {ExpectedKey}, Actual: {ActualKey}", request.TaskCode, task.WcsKey, key);
                return false;
            }
        }

        var mappings = await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
            .Where(m => m.InboundId == task.Id)
            .ToListAsync(cancellationToken);

        if (mappings.Count == 0)
        {
            _logger.LogError("No mappings found for TaskId: {TaskId}", task.Id);
            return false;
        }


        var detailIds = mappings.Select(m => m.ReceiptDetailId).ToList();

        await _actionLogService.AddLogAsync(
            $"[Update Success] Receipt  {mappings.Count} rows, detailIds={string.Join(",", detailIds)}",
            "Integration", currentUser);

        var details = await _dbContext.GetDbSet<InboundReceiptDetailEntity>()
            .Include(d => d.Receipt)
            .Where(d => detailIds.Contains(d.Id))
            .ToListAsync(cancellationToken);

        if (details.Count == 0) return false;

        var receipt = details.First().Receipt;

        // bussiness flow 
        // handle stock for put away
        await UpdateStockAsync(details, task, receipt.Id, currentUser, cancellationToken);
        if (task.LocationId > 0)
        {
            await _dbContext.GetDbSet<GoodslocationEntity>()
                .Where(l => l.Id == task.LocationId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LocationStatus, (byte)GoodLocationStatusEnum.OCCUPIED), cancellationToken);
        }

        task.Status = IntegrationStatus.Done;
        task.FinishedDate = DateTime.UtcNow;
        _dbContext.Inbounds.Update(task);


        await UpdateReceiptStatusAsync(receipt, (int)task.Id, cancellationToken);

        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Inbound,
            PalletCode = task.PalletCode,
            Status = IntegrationStatus.Done,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            FinishedDate = DateTime.UtcNow,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate,
            Description = $"Put-away success. Created Stock for Pallet {task.PalletCode}"
        }, cancellationToken);


        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }


    #region Handle Bussiness logic for Confirm Inbound

    private async Task UpdateStockAsync(List<InboundReceiptDetailEntity> details,
                                        InboundEntity task,
                                        int receiptId,
                                        CurrentUser currentUser,
                                        CancellationToken cancellationToken)
    {

        var stockDbSet = _dbContext.GetDbSet<StockEntity>();
        var uomIds = details.Select(d => d.SkuUomId).Distinct().ToList();

        var skuIds = details.Select(d => d.SkuId).Distinct().ToList();

        var skuUomLinks = await _dbContext.GetDbSet<SkuUomLinkEntity>()
                                          .Include(l => l.SkuUom)
                                          .Where(l => skuIds.Contains(l.SkuId) && uomIds.Contains(l.SkuUomId))
                                          .ToDictionaryAsync(l => (l.SkuId, l.SkuUomId), cancellationToken);

        var existingStocks = await stockDbSet
                            .Where(s => skuIds.Contains(s.sku_id)
                                     && s.goods_location_id == task.LocationId
                                     && s.Palletcode == task.PalletCode)
                            .ToListAsync(cancellationToken);

        var newStockList = new List<StockEntity>();
        var stockTransactionMappings = new List<(InboundReceiptDetailEntity detail, StockEntity stock, int conversionRate, int skuUomId, string unitName)>();

        foreach (var detail in details)
        {
            decimal inputQty = detail.Quantity;
            decimal baseQty = inputQty;
            int currentConversionRate = 1;
            string currentUnitName = string.Empty;

            if (skuUomLinks.TryGetValue((detail.SkuId, detail.SkuUomId), out var linkInfo))
            {
                currentConversionRate = linkInfo.ConversionRate;
                currentUnitName = linkInfo.SkuUom?.UnitName ?? string.Empty;

                if (!linkInfo.IsBaseUnit && linkInfo.ConversionRate > 0)
                {
                    baseQty = inputQty * linkInfo.ConversionRate;
                }
            }

            var existing = existingStocks.FirstOrDefault(s =>
                s.sku_id == detail.SkuId &&
                s.SupplierId == detail.Receipt.SupplierId &&
                s.goods_location_id == detail.LocationId &&
                s.expiry_date.GetValueOrDefault().Date == detail.ExpiryDate.GetValueOrDefault().Date);

            if (existing != null)
            {
                existing.actual_qty += baseQty;
                existing.qty += (int)baseQty;
                existing.last_update_time = DateTime.UtcNow;

                stockTransactionMappings.Add((detail, existing, currentConversionRate, detail.SkuUomId, currentUnitName));
            }
            else
            {
                var newStock = new StockEntity
                {
                    Palletcode = task.PalletCode.Trim(),
                    goods_location_id = task.LocationId,
                    sku_id = detail.SkuId,
                    qty = (int)baseQty,
                    actual_qty = baseQty,
                    PutAwayDate = DateTime.UtcNow,
                    expiry_date = detail.ExpiryDate,
                    TenantId = currentUser.tenant_id,
                    last_update_time = DateTime.UtcNow,
                    is_freeze = false,
                    SupplierId = detail.Receipt.SupplierId,
                };
                newStockList.Add(newStock);
                stockTransactionMappings
                    .Add((detail, newStock, currentConversionRate, detail.SkuUomId, currentUnitName));
            }
        }

        if (newStockList.Count > 0)
        {
            await stockDbSet.AddRangeAsync(newStockList, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var supplierId = details.FirstOrDefault()?.Receipt?.SupplierId ?? 0;
        string supplierName = string.Empty;
        if (supplierId > 0)
        {
            supplierName = await _dbContext.GetDbSet<SupplierEntity>()
                .Where(s => s.Id == supplierId)
                .Select(s => s.supplier_name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        }

        var transactionRequests = new List<AddStockTransactionRequest>();
        foreach (var mapping in stockTransactionMappings)
        {
            _dbContext.GetDbSet<StockTransactionEntity>().Add(new StockTransactionEntity()
            {
                StockId = mapping.stock.Id,
                Quantity = mapping.detail.Quantity,
                SkuId = mapping.detail.SkuId,
                SkuUomId = mapping.detail.SkuUomId,
                TransactionType = StockTransactionType.Inbound,
                TenantId = currentUser.tenant_id,
                SupplierId = mapping.detail.Receipt?.SupplierId,
                CurrentConversionRate = mapping.conversionRate,
                UnitName = mapping.unitName,
                RefReceipt = mapping.detail.Receipt?.ReceiptNumber ?? string.Empty,
                SupplierName = supplierName
            });
        }
    }

    /// <summary>
    /// Handle update receipt status after confirm put-away
    /// </summary>
    /// <param name="receipt"></param>
    /// <param name="currentTaskId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateReceiptStatusAsync(InboundReceiptEntity receipt,
                                                int currentTaskId,
                                                CancellationToken cancellationToken)
    {
        var allDetailIdsOfReceipt = await _dbContext.GetDbSet<InboundReceiptDetailEntity>()
            .Where(d => d.ReceiptId == receipt.Id)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        var allTaskIdsOfReceipt = await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
            .Where(m => allDetailIdsOfReceipt.Contains(m.ReceiptDetailId))
            .Select(m => m.InboundId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var hasPendingTasks = await _dbContext.Inbounds
            .AnyAsync(t => allTaskIdsOfReceipt.Contains((int)t.Id)
                        && t.Id != currentTaskId
                        && t.Status != IntegrationStatus.Done
                        && t.IsActive, cancellationToken);

        // 4. Cập nhật trạng thái
        if (!hasPendingTasks)
        {
            receipt.Status = ReceiptStatus.COMPLETE;
            receipt.LastUpdateTime = DateTime.UtcNow;
            _dbContext.GetDbSet<InboundReceiptEntity>().Update(receipt);

            _logger.LogInformation("Receipt {ReceiptNo} fully put-away and Completed.", receipt.ReceiptNumber);
        }
        else if (receipt.Status != ReceiptStatus.PROCESSING)
        {
            receipt.Status = ReceiptStatus.PROCESSING;
            _dbContext.GetDbSet<InboundReceiptEntity>().Update(receipt);
        }
    }

    #endregion

    /// <summary>
    /// Updates the status of an outbound task based on the provided request.
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="cancellationToken">Cancellation token</param>  
    /// <returns>True if update was successful, false otherwise</returns>
    public async Task<bool> UpdateOutboundStatusAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogWarning("UpdateOutboundStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Outbounds
            .Where(x => x.TaskCode == request.TaskCode && x.Status != IntegrationStatus.Done)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Outbound task not found for TaskCode: {TaskCode}", request.TaskCode);
            return false;
        }

        if (!string.IsNullOrEmpty(key))
        {
            if (key != task.WcsKey)
            {
                _logger.LogWarning("WCS Key mismatch for TaskCode: {TaskCode}. Expected: {ExpectedKey}, Actual: {ActualKey}", request.TaskCode, task.WcsKey, key);
                return false;
            }
        }

        await _actionLogService.AddLogAsync(
            $"[Update Outbounds Done] ReceiptId {task.Id} rows, Status={task.Status}, TaskCode={request.TaskCode}",
            "Integration", currentUser);

        task.Status = IntegrationStatus.Done;
        task.FinishedDate = DateTime.UtcNow;
        _dbContext.Outbounds.Update(task);

        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Outbound,
            PalletCode = task.PalletCode,
            Status = task.Status,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            FinishedDate = task.FinishedDate,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate
        }, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    /// <summary>
    /// Get InboundTask by using Status
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <param name="cancellationToken"></param>
    /// <param name="currentUser">  </param>
    public async Task<IEnumerable<InboundTaskResponse>?> GetInboundTaskByStatusAsync(InboundTaskConditionRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var query = _dbContext.Inbounds
        .AsNoTracking()
        .Where(x => x.Status == request.Status);
        if (request.From.HasValue)
        {
            var fromDate = request.From.Value.Date;
            query = query.Where(x => x.CreatedDate >= fromDate);
        }

        if (request.To.HasValue)
        {
            var toDate = request.To.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedDate <= toDate);
        }

        var inbounds = await query.ToListAsync();

        if (inbounds.Count == 0) return [];

        var locationIds = inbounds.Select(x => x.LocationId).Distinct().ToList();

        var palletCodes = inbounds.Select(x => x.PalletCode).Distinct().ToList();

        var locations = await _dbContext.GetDbSet<GoodslocationEntity>()
            .AsNoTracking()
            .Where(l => locationIds.Contains(l.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var warehouseIds = locations.Select(l => l.WarehouseId).Distinct().ToList();

        var warehouses = await _dbContext.GetDbSet<WarehouseEntity>(currentUser.tenant_id)
            .Where(w => warehouseIds.Contains(w.Id))
            .ToListAsync(cancellationToken);

        var asnSorts = await _dbContext.GetDbSet<AsnsortEntity>()
            .AsNoTracking()
            .Where(a => palletCodes.Contains(a.series_number))
            .ToListAsync(cancellationToken);


        var asnIds = asnSorts.Select(a => a.asn_id).Distinct().ToList();
        var asnList = await _dbContext.GetDbSet<AsnEntity>()
            .AsNoTracking()
            .Where(a => asnIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        var skuIds = asnList.Select(a => a.sku_id).Distinct().ToList();
        var skuList = await _dbContext.GetDbSet<SkuEntity>()
            .AsNoTracking()
            .Where(s => skuIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var palletFullInfors = (from sort in asnSorts
                                join asn in asnList on sort.asn_id equals asn.Id
                                join sku in skuList on asn.sku_id equals sku.Id into skuGroup
                                from sku in skuGroup.DefaultIfEmpty()
                                select new
                                {
                                    PalletCode = sort.series_number,
                                    AsnNo = asn.asn_no,
                                    Owner = asn.goods_owner_name,
                                    SkuCode = sku?.sku_code
                                }).ToList();

        var result = inbounds
            .GroupBy(x => x.TaskCode)
            .Select(g =>
            {
                var firstItem = g.First();

                var palletInfo = palletFullInfors.FirstOrDefault(p => p.PalletCode == firstItem.PalletCode);

                return new InboundTaskResponse
                {
                    TaskCode = firstItem.TaskCode,
                    PickDate = firstItem.PickUpDate.GetValueOrDefault(),
                    ResponseDate = DateTime.UtcNow,
                    AsnNumber = palletInfo?.AsnNo ?? "",
                    GoodOwnerName = palletInfo?.Owner ?? "",
                    SkuCode = palletInfo?.SkuCode ?? "",

                    Details = [.. g.Select(x =>
                    {
                        var location = locations.FirstOrDefault(l => l.Id == x.LocationId);
                        var warehouse = warehouses.FirstOrDefault(w => w.Id == location?.WarehouseId);
                        var blockId = warehouse?.WcsBlockId;

                        return new InboundTaskDTO
                        {
                            PalletCode = x.PalletCode,
                            Status = x.Status.ToString(),
                            Location = location != null
                                    ? $"{location.CoordinateZ}.{location.CoordinateX}.{location.CoordinateY}"
                                    : string.Empty,
                            BlockId = blockId ?? string.Empty
                        };
                    })]
                };
            })
            .ToList();

        return result;
    }

    /// <summary>
    /// Reject Task for Inbound
    /// </summary>
    /// <param name="request"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<bool> RejectInboundTaskAsync(InboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (request == null)
        {
            _logger.LogWarning("UpdateStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Inbounds
            .FirstOrDefaultAsync(x => x.TaskCode == request.TaskCode
                       && x.PalletCode == request.PalletCode, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Inbound task not found for PalletCode: {PalletCode}", request.PalletCode);
            return false;
        }

        if (!string.IsNullOrEmpty(key))
        {
            if (key != task.WcsKey)
            {
                _logger.LogWarning("WCS Key mismatch for TaskCode: {TaskCode}. Expected: {ExpectedKey}, Actual: {ActualKey}", request.TaskCode, task.WcsKey, key);
                return false;
            }
        }

        await _actionLogService.AddLogAsync(
            $"[RejectInbound] ReceiptId {task.Id} rows, Status={task.Status}, TaskCode={request.TaskCode} PalletCode={request.PalletCode}",
            "Integration", currentUser);

        task.Status = IntegrationStatus.Reject;
        task.FinishedDate = DateTime.UtcNow;
        task.IsActive = false;
        _dbContext.Inbounds.Update(task);

        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Inbound,
            PalletCode = task.PalletCode,
            Status = task.Status,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            FinishedDate = DateTime.UtcNow,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate,
            Description = "Reject by WCS"

        }, cancellationToken);

        var pallet = await _dbContext.GetDbSet<PalletEntity>()
                                     .Where(t => t.PalletCode == request.PalletCode)
                                     .FirstOrDefaultAsync(cancellationToken);
        if (pallet == null)
        {
            _logger.LogError("Pallet not found for PalletCode: {PalletCode}", request.PalletCode);
            return false;
        }

        pallet.PalletStatus = PalletEnumStatus.Available;
        _dbContext.GetDbSet<PalletEntity>().Update(pallet);


        var detailIdsLinkedToTask = await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
                                                    .Where(mapping => mapping.InboundId == task.Id)
                                                    .Select(mapping => mapping.ReceiptDetailId)
                                                    .ToListAsync(cancellationToken);

        if (detailIdsLinkedToTask.Any())
        {
            await _dbContext.GetDbSet<InboundReceiptDetailEntity>()
                .Where(d => detailIdsLinkedToTask.Contains(d.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.LocationId, (int?)null)
                    .SetProperty(p => p.PalletCode, (string?)null)
                , cancellationToken);

            var receiptIds = await _dbContext.GetDbSet<InboundReceiptDetailEntity>()
                .Where(d => detailIdsLinkedToTask.Contains(d.Id))
                .Select(d => d.ReceiptId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (receiptIds.Count > 0)
            {
                var receipts = await _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id, true)
                    .Where(r => receiptIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

                foreach (var receipt in receipts)
                {
                    receipt.Status = ReceiptStatus.PROCESSING;
                    receipt.LastUpdateTime = DateTime.UtcNow;
                }

                _dbContext.GetDbSet<InboundReceiptEntity>().UpdateRange(receipts);
            }
        }

        await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
                        .Where(mapping => mapping.InboundId == task.Id)
                        .ExecuteDeleteAsync(cancellationToken);


        if (task.LocationId > 0)
        {
            await _dbContext.GetDbSet<GoodslocationEntity>()
                .Where(l => l.Id == task.LocationId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LocationStatus, (byte)GoodLocationStatusEnum.AVAILABLE), cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Reject Outbound task
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <returns>True if rejection was successful, false otherwise</returns>
    public async Task<bool> RejectOutboundTaskAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (request == null)
        {
            _logger.LogWarning("UpdateStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Outbounds
            .FirstOrDefaultAsync(x => x.TaskCode == request.TaskCode
                       && x.PalletCode == request.PalletCode, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Outbound task not found for PalletCode: {PalletCode}", request.PalletCode);
            return false;
        }


        if (!string.IsNullOrEmpty(key))
        {
            if (key != task.WcsKey)
            {
                _logger.LogWarning("WCS Key mismatch for TaskCode: {TaskCode}. Expected: {ExpectedKey}, Actual: {ActualKey}", request.TaskCode, task.WcsKey, key);
                return false;
            }

        }
        else if (!string.IsNullOrEmpty(task.WcsKey))
        {
            await _actionLogService.AddLogAsync(
            $"[NO-ALLOW-Reject-Outbound] ReceiptId {task.Id} rows, Status={task.Status}, TaskCode={request.TaskCode} PalletCode={request.PalletCode}",
            "Integration", currentUser);
            _logger.LogWarning("WCS Key is required for TaskCode: {TaskCode} but was not provided", request.TaskCode);
            return false;
        }

        await _actionLogService.AddLogAsync(
            $"[RejectOutbound] ReceiptId {task.Id} rows, Status={task.Status}, TaskCode={request.TaskCode} PalletCode={request.PalletCode}",
            "Integration", currentUser);

        task.Status = IntegrationStatus.Reject;
        task.FinishedDate = DateTime.UtcNow;
        _dbContext.Outbounds.Update(task);

        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Outbound,
            PalletCode = task.PalletCode,
            Status = task.Status,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            FinishedDate = DateTime.UtcNow,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate
        }, cancellationToken);


        var detailIdsLinkedToTask = await _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
                                                          .Where(mapping => mapping.OutboundId == task.Id)
                                                          .Select(mapping => mapping.ReceiptDetailId)
                                                          .ToListAsync(cancellationToken);

        if (detailIdsLinkedToTask.Count != 0)
        {
            await _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
                .Where(d => detailIdsLinkedToTask.Contains(d.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.LocationId, (int?)null)
                    .SetProperty(p => p.PalletCode, (string?)null)
                , cancellationToken);

            var receiptIds = await _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
                .Where(d => detailIdsLinkedToTask.Contains(d.Id))
                .Select(d => d.ReceiptId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (receiptIds.Count > 0)
            {
                var receipts = await _dbContext.GetDbSet<OutBoundReceiptEntity>(currentUser.tenant_id, true)
                    .Where(r => receiptIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

                foreach (var receipt in receipts)
                {
                    // need user chose another location for outbound receipt
                    receipt.Status = ReceiptStatus.DRAFT;
                    receipt.LastUpdateTime = DateTime.UtcNow;
                    receipt.Description = $"Reject by WCS | {receipt.Description}".Trim();
                }

                _dbContext.GetDbSet<OutBoundReceiptEntity>().UpdateRange(receipts);
            }
        }

        await _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
                        .Where(mapping => mapping.OutboundId == task.Id)
                        .ExecuteDeleteAsync(cancellationToken);


        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }


    /// <summary>
    /// Get OutboundTask by using Status and date range
    /// </summary>
    /// <param name="request">Filter conditions</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="cancellationToken">Cancellation token</param>      
    /// <returns>Filtered outbound tasks</returns>
    public async Task<IEnumerable<OutboundTaskResponse>?> GetOutboundTaskByStatusAsync(OutboundTaskConditionRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var query = _dbContext.Outbounds
            .AsNoTracking()
            .Where(x => x.Status == request.Status);

        if (request.From.HasValue)
        {
            var fromDate = request.From.Value.Date;
            query = query.Where(x => x.CreatedDate >= fromDate);
        }

        if (request.To.HasValue)
        {
            var toDate = request.To.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedDate <= toDate);
        }

        var outbounds = await query.ToListAsync();

        if (outbounds.Count == 0) return [];

        var locationIds = outbounds.Select(x => x.LocationId).Distinct().ToList();
        var palletCodes = outbounds.Select(x => x.PalletCode).Distinct().ToList();

        var locations = await _dbContext.GetDbSet<GoodslocationEntity>()
           .AsNoTracking()
           .Where(l => locationIds.Contains(l.Id))
           .ToListAsync(cancellationToken: cancellationToken);

        var warehouseIds = locations.Select(l => l.WarehouseId).Distinct().ToList();

        var warehouses = await _dbContext.GetDbSet<WarehouseEntity>(currentUser.tenant_id)
            .Where(w => warehouseIds.Contains(w.Id))
            .ToListAsync(cancellationToken);

        var dispatchPickLists = await _dbContext.GetDbSet<DispatchpicklistEntity>()
            .AsNoTracking()
            .Where(d => palletCodes.Contains(d.series_number))
            .ToListAsync();

        var dispatchListIds = dispatchPickLists.Select(d => d.dispatchlist_id).Distinct().ToList();
        var dispatchLists = await _dbContext.GetDbSet<DispatchlistEntity>()
            .AsNoTracking()
            .Where(d => dispatchListIds.Contains(d.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var skuIds = dispatchPickLists.Select(d => d.sku_id).Distinct().ToList();
        var skus = await _dbContext.GetDbSet<SkuEntity>()
            .AsNoTracking()
            .Where(s => skuIds.Contains(s.Id))
            .ToListAsync();

        var palletFullInfos = (from dp in dispatchPickLists
                               join d in dispatchLists on dp.dispatchlist_id equals d.Id
                               join s in skus on dp.sku_id equals s.Id
                               select new
                               {
                                   PalletCode = dp.series_number,
                                   SoNumber = d.dispatch_no,
                                   Owner = d.customer_name,
                                   SkuCode = s.sku_code,
                               }).ToList();

        var result = outbounds
                   .GroupBy(x => x.TaskCode)
                   .Select(g =>
                   {
                       var firstItem = g.First();
                       var palletInfo = palletFullInfos.FirstOrDefault(p => p.PalletCode == firstItem.PalletCode);
                       return new OutboundTaskResponse
                       {
                           TaskCode = firstItem.TaskCode,
                           PickDate = firstItem.PickUpDate.GetValueOrDefault(),
                           ResponseDate = DateTime.UtcNow,
                           SoNumber = palletInfo?.SoNumber ?? "",
                           CustomerName = palletInfo?.Owner ?? "",
                           SkuCode = palletInfo?.SkuCode ?? "",
                           Details = [.. g.Select(x =>
                    {
                           var location = locations.FirstOrDefault(l => l.Id == x.LocationId);
                           var warehouse = warehouses.FirstOrDefault(w => w.Id == location?.WarehouseId);
                           var blockId = warehouse?.WcsBlockId;
                            return new OutboundTaskDTO
                            {
                                PalletCode = x.PalletCode,
                                Status = x.Status.ToString(),
                                Location = location != null
                                        ? $"{location.CoordinateZ}.{location.CoordinateX}.{location.CoordinateY}"
                                        : string.Empty,
                                BlockId = blockId ?? string.Empty,
                            };
                        }
                    )
                           ]
                       };
                   })
                   .ToList();

        return result;
    }

    /// <summary>
    /// Updates Outbound success Status 
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <returns>True if update was successful, false otherwise</returns>
    public async Task<bool> UpdateOutboundSuccessStatusAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (request == null)
        {
            _logger.LogWarning("UpdateStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Outbounds
            .FirstOrDefaultAsync(x => x.TaskCode == request.TaskCode
                       && x.PalletCode == request.PalletCode
                       && x.Status != IntegrationStatus.Done
                       && x.IsActive, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Inbound task not found for PalletCode: {PalletCode}", request.PalletCode);
            return false;
        }

        if (!string.IsNullOrEmpty(key))
        {
            if (key != task.WcsKey)
            {
                _logger.LogWarning("WCS Key mismatch for TaskCode: {TaskCode}. Expected: {ExpectedKey}, Actual: {ActualKey}", request.TaskCode, task.WcsKey, key);
                return false;
            }
        }

        await _actionLogService.AddLogAsync(
            $"[Outbound Success {key}] ReceiptId {task.Id} rows, Status={task.Status}, TaskCode={request.TaskCode} PalletCode={request.PalletCode}",
            "Integration", currentUser);

        task.Status = IntegrationStatus.Done;
        task.FinishedDate = DateTime.UtcNow;
        _dbContext.Outbounds.Update(task);

        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Outbound,
            PalletCode = task.PalletCode,
            Status = task.Status,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            FinishedDate = DateTime.UtcNow,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate
        }, cancellationToken);

        var integrationDetailIds = await _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
                                                   .Where(i => i.OutboundId == task.Id)
                                                   .Select(i => i.ReceiptDetailId)
                                                   .ToListAsync(cancellationToken);

        if (integrationDetailIds.Count != 0)
        {
            var receiptDetails = await _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
                                                 .Include(d => d.Receipt)
                                                 .Where(d => integrationDetailIds.Contains(d.Id))
                                                 .ToListAsync(cancellationToken);

            var receipt = receiptDetails.First().Receipt;

            var stockDeductItems = receiptDetails.Select(d => new UpdateOutboundReceiptDetailDto
            {
                SkuId = d.SkuId,
                Quantity = d.Quantity,
                SkuUomId = d.SkuUomId,
                LocationId = d.LocationId,
                PalletCode = d.PalletCode
            }).ToList();

            await DeductStockAsync(stockDeductItems, receipt.ReceiptNumber, receipt.CustomerId, tenantId, cancellationToken);

            var affectedLocationIds = receiptDetails.Where(d => d.LocationId.HasValue).Select(d => d.LocationId.GetValueOrDefault()).Distinct().ToList();
            var affectedPalletCodes = receiptDetails.Where(d => !string.IsNullOrWhiteSpace(d.PalletCode)).Select(d => d.PalletCode).Distinct().ToList();

            if (affectedLocationIds.Count != 0)
            {
                await ReleaseLocationsIfEmptyAsync(affectedLocationIds, tenantId, cancellationToken);
            }

            if (affectedPalletCodes.Count != 0)
            {
                await ReleasePalletsIfEmptyAsync(affectedPalletCodes, tenantId, cancellationToken);
            }

            var receiptId = receipt.Id;

            var allReceiptDetailIds = await _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
                    .Where(d => d.ReceiptId == receiptId)
                    .Select(d => d.Id)
                    .ToListAsync(cancellationToken);

            var hasPendingTasks = await (from i in _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
                                         join o in _dbContext.Outbounds on i.OutboundId equals o.Id
                                         where allReceiptDetailIds.Contains(i.ReceiptDetailId)
                                            && o.Status != IntegrationStatus.Done
                                            && o.Id != task.Id
                                         select o.Id).AnyAsync(cancellationToken);


            if (!hasPendingTasks)
            {
                receipt.Status = ReceiptStatus.COMPLETE;
                receipt.LastUpdateTime = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }


    #region Bussiness logic for confirm outbound

    private async Task DeductStockAsync(List<UpdateOutboundReceiptDetailDto> details,
                                        string receiptNumber,
                                        int? customerId,
                                        long tenantId,
                                        CancellationToken cancellationToken)
    {
        var stockEntities = _dbContext.GetDbSet<StockEntity>(tenantId, isTracking: true);
        var skuIds = details.Select(d => d.SkuId).Distinct().ToList();
        var uomIds = details.Select(d => d.SkuUomId).Distinct().ToList();
        var skuUomLinks = await _dbContext.GetDbSet<SkuUomLinkEntity>()
                                          .Include(l => l.SkuUom)
                                          .Where(l => skuIds.Contains(l.SkuId) && uomIds.Contains(l.SkuUomId))
                                          .ToDictionaryAsync(l => (l.SkuId, l.SkuUomId), cancellationToken);

        var transactionRequests = new List<AddStockTransactionRequest>();
        var customerName = _dbContext.GetDbSet<CustomerEntity>(tenantId)
                                     .Where(c => c.Id == customerId)
                                     .Select(c => c.customer_name)
                                     .FirstOrDefault();

        foreach (var item in details)
        {
            decimal inputQty = item.Quantity;
            decimal baseQty = inputQty;
            int currentConversionRate = 1;
            string currentUnitName = string.Empty;
            if (skuUomLinks.TryGetValue((item.SkuId, item.SkuUomId), out var linkInfo))
            {
                currentConversionRate = linkInfo.ConversionRate;
                currentUnitName = linkInfo.SkuUom?.UnitName ?? string.Empty;

                if (!linkInfo.IsBaseUnit && linkInfo.ConversionRate > 0)
                {
                    baseQty = inputQty * linkInfo.ConversionRate;
                }
            }
            var palletCode = item.PalletCode?.Trim() ?? string.Empty;

            var stock = await stockEntities
                .FirstAsync(s =>
                    s.sku_id == item.SkuId
                    && s.goods_location_id == item.LocationId
                    && s.Palletcode == palletCode, cancellationToken);

            stock.actual_qty -= baseQty;
            stock.qty -= (int)baseQty;
            stock.last_update_time = DateTime.UtcNow;

            if (stock.actual_qty < 0)
            {
                stock.actual_qty = 0;
            }
            if (stock.qty < 0)
            {
                stock.qty = 0;
            }

            _dbContext.GetDbSet<StockTransactionEntity>()
                .Add(new StockTransactionEntity()
                {
                    StockId = stock.Id,
                    Quantity = -item.Quantity,
                    SkuId = item.SkuId,
                    SkuUomId = item.SkuUomId,
                    TransactionType = StockTransactionType.Outbound,
                    TenantId = tenantId,
                    CustomerId = customerId,
                    CustomerName = customerName,
                    CurrentConversionRate = currentConversionRate,
                    UnitName = currentUnitName,
                    RefReceipt = receiptNumber,
                    TransactionDate = DateTime.UtcNow
                });
        }

    }



    private static decimal ConvertToBaseQty(
       decimal quantity, int uomId, Dictionary<int, SkuUomEntity> uoms)
    {
        return quantity;

        //if (!uoms.TryGetValue(uomId, out var uom) || uom.IsBaseUnit)
        //{
        //    return quantity;
        //}

        //return uom.Operator == ConversionOperator.Multiply
        //    ? quantity * uom.ConversionRate
        //    : quantity / uom.ConversionRate;
    }


    private async Task ReleaseLocationsIfEmptyAsync(List<int> locationIds, long tenantId,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0) return;

        // 1. Tìm các vị trí thực tế VẪN CÒN HÀNG (của bất kỳ SKU nào)
        var locationsStillHavingStock = await _dbContext.GetDbSet<StockEntity>(tenantId)
            .Where(s => locationIds.Contains(s.goods_location_id) && (s.qty > 0 || s.actual_qty > 0))
            .Select(s => s.goods_location_id)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 2. Lọc ra các vị trí THỰC SỰ TRỐNG (Tổng truyền vào - Số vị trí còn hàng)
        var actuallyEmptyLocationIds = locationIds.Except(locationsStillHavingStock).ToList();

        if (actuallyEmptyLocationIds.Count == 0) return;

        // 3. Giải phóng các vị trí thực sự trống
        var locationsToRelease = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId, isTracking: true)
            .Where(l => actuallyEmptyLocationIds.Contains(l.Id)
                && l.LocationStatus != (byte)GoodLocationStatusEnum.AVAILABLE)
            .ToListAsync(cancellationToken);

        foreach (var location in locationsToRelease)
        {
            location.LocationStatus = (byte)GoodLocationStatusEnum.AVAILABLE;
            location.LastUpdateTime = DateTime.UtcNow;
        }
    }


    private async Task ReleasePalletsIfEmptyAsync(
    List<string> palletCodes,
    long tenantId,
    CancellationToken cancellationToken)
    {
        if (palletCodes == null || palletCodes.Count == 0) return;

        // Đảm bảo dữ liệu đầu vào sạch
        var cleanPalletCodes = palletCodes.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList();
        if (cleanPalletCodes.Count == 0) return;

        // 1. Tìm các Pallet thực tế VẪN CÒN HÀNG
        var palletsStillHavingStock = await _dbContext.GetDbSet<StockEntity>(tenantId)
            .Where(s => cleanPalletCodes.Contains(s.Palletcode) && (s.actual_qty > 0 || s.qty > 0))
            .Select(s => s.Palletcode)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 2. Lọc ra các Pallet THỰC SỰ TRỐNG
        var actuallyEmptyPalletCodes = cleanPalletCodes.Except(palletsStillHavingStock).ToList();

        if (actuallyEmptyPalletCodes.Count == 0) return;

        // 3. Giải phóng các Pallet thực sự trống
        var palletsToRelease = await _dbContext.GetDbSet<PalletEntity>()
            .Where(p => actuallyEmptyPalletCodes.Contains(p.PalletCode) && p.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var pallet in palletsToRelease)
        {
            pallet.PalletStatus = PalletEnumStatus.Available;
            pallet.IsFull = false;

            // Tùy nghiệp vụ của bạn: Nếu giải phóng pallet thì có tháo pallet ra khỏi vị trí luôn không?
            // Nếu có thì uncomment dòng dưới:
            // pallet.GoodsLocationId = 0; 
        }
    }

    #endregion

    /// <summary>
    /// Create out bound task
    /// </summary>
    /// <param name="tasks"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<List<OutboundEntity>> CreateOutboundEntitiesAsync(
      List<CreateOutboundTaskDTO> tasks,
      CurrentUser currentUser)
    {
        if (tasks == null || tasks.Count == 0)
        {
            return [];
        }

        // handle multiple palletcode with the same task code
        var shareTaskCode = GenarationHelper.GenerateTaskCode();
        var outboundEntities = tasks.Select(dto => new OutboundEntity
        {
            TenantId = currentUser.tenant_id,
            PalletCode = dto.PalletCode,
            TaskCode = shareTaskCode,
            LocationId = dto.LocationId,
            CreatedDate = DateTime.UtcNow,
            PickUpDate = dto.PickUpDate,
            Priority = dto.Priority,
            Status = IntegrationStatus.Processing,
            IsActive = dto.IsActive,
        }).ToList();

        await _dbContext.Outbounds.AddRangeAsync(outboundEntities);
        var result = await _dbContext.SaveChangesAsync();

        if (result <= 0)
        {
            _logger.LogError("Failed to create inbound entities for User: {UserId}", currentUser.user_id);
            return [];
        }

        await _actionLogService.AddLogAsync(
            $"[Create Outbound Entities] Receipts {outboundEntities.Count} rows, PalletCodes={string.Join(";", tasks.Select(x => x.PalletCode))} => {string.Join(";", outboundEntities.Select(x => x.PalletCode))}",
            "Integration", currentUser);

        return outboundEntities;
    }

    /// <summary>
    /// Outbound create history
    /// </summary>
    /// <param name="outboundEntities"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<bool> CreateOutboundIntegrationHistoryAsync(List<OutboundEntity> outboundEntities, CurrentUser currentUser)
    {
        if (outboundEntities == null)
        {
            return false;
        }

        var historyEntities = outboundEntities.Select(inbound => new IntegrationHistory
        {
            Status = IntegrationStatus.Processing,
            CreatedDate = DateTime.UtcNow,
            LocationId = inbound.LocationId,
            HistoryType = HistoryType.Outbound,
            PalletCode = inbound.PalletCode,
            Priority = inbound.Priority,
            IsActive = true,
            PickUpDate = inbound.PickUpDate,
            TaskCode = inbound.TaskCode,
            TenantId = currentUser.tenant_id,
        }).ToList();

        await _dbContext.IntegrationHistories.AddRangeAsync(historyEntities);
        var result = await _dbContext.SaveChangesAsync();

        return result > 0;
    }

    /// <summary>
    /// Get map locations
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="currentUser"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IEnumerable<LocationResponse>> GetMapLocations(Guid? blockId, CurrentUser currentUser, CancellationToken ct)
    {
        var configuration = _serviceProvider.GetService<IConfiguration>();
        string apiUrl = configuration?["WcsSettings:ApiUrl"] ?? "";
        var client = _httpClientFactory.CreateClient();
        try
        {
            string url = $"{apiUrl}/api/Locations?page=-1&pageSize=100&blockId={blockId}";
            var response = await client.GetAsync(url ?? "", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("WMS API returned status: {response}", response);
                return [];
            }

            await _actionLogService.AddLogAsync($"[Get WCS Map Locations] blockId={blockId}", "Integration", currentUser);

            //Read data
            var content = await response.Content.ReadAsStringAsync(ct);

            return TryGetLocationResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot connect to WCS API: {error}", ex.Message);
        }

        return [];
    }

    private IEnumerable<LocationResponse> TryGetLocationResponse(string content)
    {
        try
        {
            var res = JsonSerializer.Deserialize<WcsResponse>(content, options);
            return res?.Data ?? [];
        }
        catch (JsonException jex)
        {
            _logger.LogError(jex, "JsonException parsing JSON from WMS. Skipping!");
            try
            {
                var res = JsonSerializer.Deserialize<WcsPageResponse>(content, options);
                return res?.Data.Items ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JSON from WMS.");
                return [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Exception from WMS.");
            return [];
        }
    }

    /// <summary>
    /// Get Block Locations
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IEnumerable<BlockLocation>> GetBlockLocations(CancellationToken ct)
    {
        var configuration = _serviceProvider.GetService<IConfiguration>();
        string apiUrl = configuration?["WcsSettings:ApiUrl"] ?? "";
        var client = _httpClientFactory.CreateClient();
        try
        {
            string url = $"{apiUrl}/api/Blocks?page=-1&pageSize=100";
            var response = await client.GetAsync(url ?? "", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("WMS API returned status: {response}", response);
                return [];
            }

            //Read data
            var content = await response.Content.ReadAsStringAsync(ct);
            try
            {
                var res = JsonSerializer.Deserialize<WcsBlockResponse>(content, options);
                return res?.Data ?? [];
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Error parsing JSON from WMS.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot connect to WCS API: {error}", ex.Message);
        }

        return [];
    }

    /// <summary>
    /// Update Inbound processing status
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="key"></param>
    /// <param name="currentUser">Current User</param>
    /// <returns></returns>
    public async Task<bool> UpdateInboundProcessingStatusAsync(InboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (request == null)
        {
            _logger.LogWarning("UpdateInboundProcessingStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Inbounds
            .FirstOrDefaultAsync(x =>
                x.TaskCode.Trim() == request.TaskCode.Trim() &&
                x.PalletCode.Trim() == request.PalletCode.Trim() &&
                (x.Status == IntegrationStatus.Ready), cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Inbound task not found or invalid state. TaskCode={TaskCode}, Pallet={PalletCode}",
                request.TaskCode, request.PalletCode);
            return false;
        }

        if (key is not null)
        {
            task.WcsKey = key.Trim();
        }

        await _actionLogService.AddLogAsync(
            $"[Update Inbound Processing] Receipts {task.Id}, Status={task.Status}, WcsKey={task.WcsKey}, TaskCode={request.TaskCode}, PalletCode={request.PalletCode}",
            "Integration", currentUser);

        task.Status = IntegrationStatus.Processing;
        _dbContext.Inbounds.Update(task);


        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Inbound,
            PalletCode = task.PalletCode,
            Status = task.Status,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate
        }, cancellationToken);

        var receiptDetailIds = await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
            .Where(m => m.InboundId == task.Id)
            .Select(m => m.ReceiptDetailId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (receiptDetailIds.Count > 0)
        {
            var receiptIds = await _dbContext.GetDbSet<InboundReceiptDetailEntity>()
                .Where(d => receiptDetailIds.Contains(d.Id))
                .Select(d => d.ReceiptId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (receiptIds.Count > 0)
            {
                var receipts = await _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id, true)
                    .Where(r => receiptIds.Contains(r.Id) && r.Status == ReceiptStatus.NEW)
                    .ToListAsync(cancellationToken);

                foreach (var receipt in receipts)
                {
                    receipt.Status = ReceiptStatus.PROCESSING;
                    receipt.LastUpdateTime = DateTime.UtcNow;
                }

                _dbContext.GetDbSet<InboundReceiptEntity>().UpdateRange(receipts);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }


    /// <summary>
    /// Update outbound
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> UpdateOutboundProcessingStatusAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (request == null)
        {
            _logger.LogWarning("UpdateOutboundProcessingStatusAsync called with null request");
            return false;
        }

        var task = await _dbContext.Outbounds
            .FirstOrDefaultAsync(x =>
                x.TaskCode.Trim() == request.TaskCode.Trim() &&
                x.PalletCode.Trim() == request.PalletCode.Trim() &&
                (x.Status == IntegrationStatus.Ready), cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Inbound task not found or invalid state. TaskCode={TaskCode}, Pallet={PalletCode}",
                request.TaskCode, request.PalletCode);
            return false;
        }

        // handle case for key 
        if (key is not null)
        {
            task.WcsKey = key.Trim();
        }

        await _actionLogService.AddLogAsync(
            $"[Update Outbound Processing] Receipts {task.Id}, Status={task.Status}, WcsKey={task.WcsKey}, TaskCode={request.TaskCode}, PalletCode={request.PalletCode}",
            "Integration", currentUser);

        task.Status = IntegrationStatus.Processing;
        _dbContext.Outbounds.Update(task);

        await _dbContext.IntegrationHistories.AddAsync(new IntegrationHistory
        {
            HistoryType = HistoryType.Outbound,
            PalletCode = task.PalletCode,
            Status = task.Status,
            CreatedDate = DateTime.UtcNow,
            Priority = task.Priority,
            TaskCode = task.TaskCode,
            TenantId = task.TenantId,
            LocationId = task.LocationId,
            IsActive = true,
            PickUpDate = task.PickUpDate
        }, cancellationToken);

        var receiptDetailIds = await _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
            .Where(m => m.OutboundId == task.Id)
            .Select(m => m.ReceiptDetailId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (receiptDetailIds.Count > 0)
        {
            var receiptIds = await _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
                .Where(d => receiptDetailIds.Contains(d.Id))
                .Select(d => d.ReceiptId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (receiptIds.Count > 0)
            {
                var receipts = await _dbContext.GetDbSet<OutBoundReceiptEntity>(currentUser.tenant_id, true)
                    .Where(r => receiptIds.Contains(r.Id) && r.Status == ReceiptStatus.NEW)
                    .ToListAsync(cancellationToken);

                foreach (var receipt in receipts)
                {
                    receipt.Status = ReceiptStatus.PROCESSING;
                    receipt.LastUpdateTime = DateTime.UtcNow;
                }

                _dbContext.GetDbSet<OutBoundReceiptEntity>().UpdateRange(receipts);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }


    /// <summary>
    /// Get pallet Location
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<PalletLocationDto>> GetPalletLocationAsync(string blockId, CancellationToken cancellationToken)
    {
        var configuration = _serviceProvider.GetService<IConfiguration>();
        string apiUrl = configuration?["WcsSettings:ApiUrl"] ?? "";
        var client = _httpClientFactory.CreateClient();
        try
        {
            string url = $"{apiUrl}/api/Pallets/pallet-by-block?blockId={blockId}";
            var response = await client.GetAsync(url ?? "", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("WMS API returned status: {response}", response);
                return [];
            }

            //Read data
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var res = JsonSerializer.Deserialize<PalletLocationResponse>(content, options);
                return res?.Data ?? [];
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Error parsing JSON from WMS.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot connect to WCS API: {error}", ex.Message);
        }

        return [];
    }

    /// <summary>
    /// Sync Location Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="traceID"></param>
    /// <param name="sourceSystem"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> SyncLocationDataAsync(SyncLocationRequest request, string traceID, string? sourceSystem, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.BlockId))
        {
            return (false, "BlockId is required.");
        }

        var tenantId = currentUser.tenant_id;


        var source = string.IsNullOrWhiteSpace(sourceSystem) ? string.Empty : sourceSystem;
        var blockId = request.BlockId;

        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .FirstOrDefaultAsync(x => x.WcsBlockId == blockId && x.is_valid, cancellationToken);

        if (warehouse is null)
        {
            return (false, $"Cannot resolve warehouse by BlockId '{blockId}'.");
        }

        var warehouseId = warehouse.Id;
        var logDbSet = _dbContext.GetDbSet<LocationSyncConflictLogEntity>();

        var existedBatch = await logDbSet.FirstOrDefaultAsync(x =>
            x.TenantId == tenantId &&
            x.WarehouseId == warehouseId &&
            x.SourceSystem == source &&
            x.ActionTime == request.ActionTime,
            cancellationToken);

        if (existedBatch is not null)
        {
            return (true, "Duplicate batch. Request ignored.");
        }

        int inserted = 0;
        int updated = 0;


        var wmsLocationCount = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
        .Where(x => x.WarehouseId == warehouseId
                 //&& x.IsValid
                 && x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation)
        .CountAsync(cancellationToken);

        if (wmsLocationCount == 0)
        {
            _logger.LogInformation($"First sync for warehouse {warehouseId} and block {request.BlockId}");
            var newLocation = new SyncWcsLocationViewModel
            {
                WcsBlockId = request.BlockId,
                Locations = request.PalletLocations,
                WarehouseId = warehouseId
            };

            var result = await _warehouseService.SynchronousWcsLocationsAsync(newLocation, currentUser);

            await _actionLogService.AddLogAsync(
         $"[Sync Location Data] BlockId={blockId}, WarehouseId={warehouseId}, first sync, Result={result}",
         "Integration",
         currentUser);

            return result.id > 0
                ? (true, "First sync success.")
                : (false, "First sync failed.");
        }

        var upsertRequests = await BuildLocationSyncConflictsAsync(
            tenantId,
            warehouseId,
            request,
            cancellationToken);

        upsertRequests.ForEach(r => r.TraceId = traceID);

        (inserted, updated) = await _stockService.UpsertLocationSyncConflictsAsync(
            upsertRequests,
            currentUser,
            cancellationToken);


        await logDbSet.AddAsync(new LocationSyncConflictLogEntity
        {
            TraceId = traceID,
            WarehouseId = warehouseId,
            SourceSystem = source,
            TenantId = tenantId,
            ReceivedTime = DateTime.UtcNow,
            ActionTime = request.ActionTime,
            ConflictInserted = inserted,
            ConflictUpdated = updated,
            CompletedTime = DateTime.UtcNow,
            ErrorMessage = null
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _actionLogService.AddLogAsync(
            $"[Sync Location Data] BlockId={blockId}, WarehouseId={warehouseId}, TraceId={traceID}, Inserted={inserted}, Updated={updated}",
            "Integration",
            currentUser);

        return (true, $"Sync success. Inserted={inserted}, Updated={updated}");
    }


    private async Task<List<UpsertLocationSyncConflictRequest>> BuildLocationSyncConflictsAsync(long tenantId,
                                                                                                int warehouseId,
                                                                                                SyncLocationRequest request,
                                                                                                CancellationToken cancellationToken)
    {
        var locations = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
            .Where(x => x.WarehouseId == warehouseId
                     && x.IsValid
                     && x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation)
            .Select(x => new { x.Id, x.LocationName })
            .ToListAsync(cancellationToken);

        if (locations.Count == 0)
        {

            return [];
        }

        var locationByName = locations
            .GroupBy(x => x.LocationName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var wcsByPallet = (request.PalletLocations ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.PalletCode))
            .GroupBy(x => x.PalletCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First().Address ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        var wmsPalletRows = await _dbContext.GetDbSet<StockEntity>(tenantId)
                                        .Where(x => x.qty > 0 && !string.IsNullOrWhiteSpace(x.Palletcode))
                                        .Join(
                                            _dbContext.GetDbSet<GoodslocationEntity>(tenantId),
                                            s => s.goods_location_id,
                                            l => l.Id,
                                            (s, l) => new
                                            {
                                                PalletCode = s.Palletcode!,
                                                l.Id,
                                                l.LocationName,
                                                l.WarehouseId,
                                                l.IsValid,
                                                l.GoodsLocationType
                                            })
                                        .Where(x => x.WarehouseId == warehouseId
                                                 && x.IsValid
                                                 && x.GoodsLocationType != GoodsLocationTypeEnum.VirtualLocation)
                                        .ToListAsync(cancellationToken);

        var wmsByPallet = wmsPalletRows
            .GroupBy(x => x.PalletCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new { x.Id, x.LocationName }).ToList(),
                StringComparer.OrdinalIgnoreCase);


        var allPallets = wcsByPallet.Keys
            .Union(wmsByPallet.Keys, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var conflicts = new List<UpsertLocationSyncConflictRequest>();

        foreach (var palletKey in allPallets)
        {
            var hasWcs = wcsByPallet.TryGetValue(palletKey, out var wcsLocation);
            var hasWms = wmsByPallet.TryGetValue(palletKey, out var wmsRows);

            if (hasWcs && hasWms)
            {
                foreach (var wmsRow in wmsRows!)
                {
                    var isMatch = string.Equals(wcsLocation, wmsRow.LocationName, StringComparison.OrdinalIgnoreCase);
                    if (!isMatch)
                    {
                        conflicts.Add(new UpsertLocationSyncConflictRequest
                        {
                            WarehouseId = warehouseId,
                            LocationId = wmsRow.Id,
                            LocationName = wmsRow.LocationName,
                            WmsHasPallet = true,
                            WcsStatus = 1,
                            Reason = "LOCATION_MISMATCH"
                        });
                    }
                }

                continue;
            }

            if (hasWcs && !hasWms)
            {
                conflicts.Add(new UpsertLocationSyncConflictRequest
                {
                    WarehouseId = warehouseId,
                    LocationId = locationByName.TryGetValue(wcsLocation!, out var loc) ? loc.Id : 0,
                    LocationName = wcsLocation!,
                    WmsHasPallet = false,
                    WcsStatus = 1,
                    Reason = "WCS_ONLY"
                });

                continue;
            }

            foreach (var wmsRow in wmsRows!)
            {
                conflicts.Add(new UpsertLocationSyncConflictRequest
                {
                    WarehouseId = warehouseId,
                    LocationId = wmsRow.Id,
                    LocationName = wmsRow.LocationName,
                    WmsHasPallet = true,
                    WcsStatus = 0,
                    Reason = "WMS_ONLY",

                });
            }
        }

        MergeSameLocationCrossPalletConflicts(conflicts);

        return conflicts
            .Where(x => !string.IsNullOrWhiteSpace(x.LocationName) && !string.IsNullOrWhiteSpace(x.Reason))
            .GroupBy(x => new { x.WarehouseId, x.LocationId, x.LocationName, x.WmsHasPallet, x.WcsStatus, x.Reason })
            .Select(g => g.First())
            .ToList();
    }

    private void MergeSameLocationCrossPalletConflicts(List<UpsertLocationSyncConflictRequest> conflicts)
    {
        var wcsOnly = conflicts
            .Where(x => x.Reason == "WCS_ONLY" && !string.IsNullOrWhiteSpace(x.LocationName))
            .GroupBy(x => x.LocationName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var wmsOnly = conflicts
            .Where(x => x.Reason == "WMS_ONLY" && !string.IsNullOrWhiteSpace(x.LocationName))
            .GroupBy(x => x.LocationName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var sameLocs = wcsOnly.Keys.Intersect(wmsOnly.Keys, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var loc in sameLocs)
        {
            var left = wcsOnly[loc];
            var right = wmsOnly[loc];
            var pairCount = Math.Min(left.Count, right.Count);

            for (var i = 0; i < pairCount; i++)
            {
                var wcsRow = left[i];
                var wmsRow = right[i];

                conflicts.Remove(wcsRow);

                wmsRow.Reason = "PALLET_CODE_MERGE_CANDIDATE";
                wmsRow.WcsStatus = 1;
                wmsRow.WmsHasPallet = true;
            }
        }
    }

}


