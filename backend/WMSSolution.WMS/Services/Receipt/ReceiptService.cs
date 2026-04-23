using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Core.Utility;
using WMSSolution.Shared;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.Planning;
using WMSSolution.Shared.RBAC;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.Entities.ViewModels.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;
using WMSSolution.WMS.Entities.ViewModels.StockTransaction;
using WMSSolution.WMS.IServices;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.Receipt;
using WMSSolution.WMS.IServices.Warehouse;

namespace WMSSolution.WMS.Services.Receipt;

/// <summary>
/// Receipt Service
/// </summary>
/// <param name="dbContext"></param>
/// <param name="localizer"></param>
/// <param name="logger"></param>
/// <param name="functionHelper"></param>
/// <param name="actionLogService"></param>
/// <param name="palletService"></param>
/// <param name="warehouseService"></param>
public class ReceiptService(
    SqlDBContext dbContext,
    IStringLocalizer<MultiLanguage> localizer,
    ILogger<ReceiptService> logger,
    FunctionHelper functionHelper,
    IActionLogService actionLogService,
    IPalletService palletService,
    IWarehouseService warehouseService) : IReceiptService
{
    private readonly IActionLogService _actionLogService = actionLogService;
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly IStringLocalizer<MultiLanguage> _localizer = localizer;
    private readonly ILogger<ReceiptService> _logger = logger;
    private readonly FunctionHelper _functionHelper = functionHelper;
    private readonly IWarehouseService _warehouseService = warehouseService;
    private readonly IPalletService _palletService = palletService;

    /// <summary>
    /// Page search requests
    /// </summary>
    /// <param name="pageSearch">page search parameters</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<(List<InboundReceiptListResponse> data, int totals)> PageInboundReceiptAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tennantId = currentUser.tenant_id;
        var receipts = _dbContext.GetDbSet<InboundReceiptEntity>(tennantId)
            .Where(x => x.Status != ReceiptStatus.CANCELED);
        var warehouses = _dbContext.GetDbSet<WarehouseEntity>(tennantId);
        var suppliers = _dbContext.GetDbSet<SupplierEntity>(tennantId);
        var details = _dbContext.GetDbSet<InboundReceiptDetailEntity>();

        var baseQuery = from r in receipts
                        join w in warehouses on r.WarehouseId equals w.Id into wGroup
                        from w in wGroup.DefaultIfEmpty()
                        join s in suppliers on r.SupplierId equals s.Id into sGroup
                        from s in sGroup.DefaultIfEmpty()
                        select new InboundReceiptListResponse
                        {
                            Id = r.Id,
                            IsStored = r.IsStored.GetValueOrDefault(),
                            ReceiptNo = r.ReceiptNumber ?? string.Empty,
                            ReceiptType = r.Type ?? string.Empty,
                            SupplierId = r.SupplierId,
                            SupplierName = s.supplier_name ?? string.Empty,
                            WarehouseId = r.WarehouseId,
                            MultiPallets = r.MultiPallets ?? false,
                            WarehouseName = w != null ? w.WarehouseName : string.Empty,
                            Status = (int)r.Status,
                            CreateDate = r.CreateDate,
                            TotalQty = 0,
                            Description = r.Description
                        };

        var filterObjects = pageSearch.searchObjects.Where(x => x.Sort == 0).ToList();

        var sortObject = pageSearch.searchObjects.FirstOrDefault(x => x.Sort != 0);

        QueryCollection queries = [];
        if (filterObjects.Count != 0)
        {
            filterObjects.ForEach(s => queries.Add(s));
        }

        var expression = queries.AsGroupedExpression<InboundReceiptListResponse>();
        if (expression != null)
        {
            baseQuery = baseQuery.Where(expression);
        }

        int totals = await baseQuery.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return ([], 0);
        }

        IOrderedQueryable<InboundReceiptListResponse> sortedQuery;

        if (sortObject != null && !string.IsNullOrEmpty(sortObject.Name))
        {

            bool isDesc = sortObject.Sort == 2; //  1 as ASC, 2 as DESC

            sortedQuery = sortObject.Name.ToLower() switch
            {
                "receiptno" => isDesc ? baseQuery.OrderByDescending(x => x.ReceiptNo) : baseQuery.OrderBy(x => x.ReceiptNo),
                "receipttype" => isDesc ? baseQuery.OrderByDescending(x => x.ReceiptType) : baseQuery.OrderBy(x => x.ReceiptType),
                "suppliername" => isDesc ? baseQuery.OrderByDescending(x => x.SupplierName) : baseQuery.OrderBy(x => x.SupplierName),
                "warehousename" => isDesc ? baseQuery.OrderByDescending(x => x.WarehouseName) : baseQuery.OrderBy(x => x.WarehouseName),
                "createdate" => isDesc ? baseQuery.OrderByDescending(x => x.CreateDate) : baseQuery.OrderBy(x => x.CreateDate),
                "status" => isDesc ? baseQuery.OrderByDescending(x => x.Status) : baseQuery.OrderBy(x => x.Status),
                _ => baseQuery.OrderByDescending(x => x.CreateDate)
            };
        }
        else
        {
            sortedQuery = baseQuery.OrderBy(t =>
                           t.Status == 1 ? 1 :
                           t.Status == 2 ? 2 :
                           t.Status == 0 ? 3 :
                           t.Status == 3 ? 4 : 5)
                        .ThenByDescending(t => t.CreateDate);
        }

        var list = await sortedQuery
                 .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                 .Take(pageSearch.pageSize)
                 .ToListAsync(cancellationToken);

        var receiptIds = list.Select(x => x.Id).ToList();

        var detailInfoLookup = await details
            .Where(d => receiptIds.Contains(d.ReceiptId))
            .GroupBy(d => d.ReceiptId)
            .Select(g => new
            {
                ReceiptId = g.Key,
                TotalQty = g.Sum(x => (decimal?)x.Quantity) ?? 0,
                HasException = g.Any(x => x.LocationId == null && x.PalletCode == null)
            })
            .ToDictionaryAsync(x => x.ReceiptId, cancellationToken);

        foreach (var item in list)
        {
            if (detailInfoLookup.TryGetValue(item.Id, out var detailInfo))
            {
                item.TotalQty = (int)detailInfo.TotalQty;
                item.IsException = detailInfo.HasException;
            }
            else
            {
                item.TotalQty = 0;
                item.IsException = false;
            }
        }

        return (list, totals);

    }

    /// <summary>
    /// get receipt by id 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<InboundReceiptDetailedDto?> GetReceiptByIdAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var receiptEntity = await _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
                                           .Include(r => r.Details)
                                           .FirstOrDefaultAsync(x => x.Id == id && x.Status != ReceiptStatus.CANCELED, cancellationToken);

        if (receiptEntity is null)
        {
            return null;
        }

        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
                                    .FirstOrDefaultAsync(w => w.Id == receiptEntity.WarehouseId, cancellationToken);
        var supplier = await _dbContext.GetDbSet<SupplierEntity>(tenantId)
                                    .FirstOrDefaultAsync(s => s.Id == receiptEntity.SupplierId, cancellationToken);

        var detailList = receiptEntity.Details ?? [];
        var skuIds = detailList.Select(d => d.SkuId).Distinct().ToList();
        var uomIds = detailList.Select(d => d.SkuUomId).Distinct().ToList();

        var skuLookup = await _dbContext.GetDbSet<SkuEntity>().AsNoTracking()
                                        .Where(s => skuIds.Contains(s.Id))
                                        .ToDictionaryAsync(s => s.Id, s => new
                                        {
                                            Code = s.sku_code,
                                            Name = s.sku_name,
                                        }, cancellationToken);

        var uomLookup = await _dbContext.GetDbSet<SkuUomEntity>(tenantId).AsNoTracking()
                                      .Where(u => uomIds.Contains(u.Id))
                                      .ToDictionaryAsync(u => u.Id, u => u.UnitName, cancellationToken);

        var locationIds = detailList
                         .Where(d => d != null && d.LocationId.HasValue)
                         .Select(d => d.LocationId!.Value)
                         .Distinct()
                         .ToList();

        var locationLookup = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                                             .Where(l => locationIds.Contains(l.Id))
                                             .ToDictionaryAsync(
                                                 l => l.Id,
                                                 l => l.LocationName ?? string.Empty,
                                                 cancellationToken
                                             );

        var sharingUrl = await _dbContext.GetDbSet<SharedReceiptEntity>(tenantId)
            .Where(x => x.InboundReceipt && x.ReceiptId == id)
            .Select(x => x.ShareKey)
            .FirstOrDefaultAsync(cancellationToken);

        return new InboundReceiptDetailedDto
        {
            Id = receiptEntity.Id,
            ReceiptNo = receiptEntity.ReceiptNumber,
            WarehouseId = receiptEntity.WarehouseId,
            WarehouseName = warehouse?.WarehouseName ?? string.Empty,
            SupplierId = receiptEntity.SupplierId,
            SupplierName = supplier?.supplier_name ?? string.Empty,
            Type = receiptEntity.Type ?? string.Empty,
            IsStored = receiptEntity.IsStored.GetValueOrDefault(),
            CreatedDate = receiptEntity.CreateDate,
            Status = (int)receiptEntity.Status,
            Description = receiptEntity.Description,
            Deliverer = receiptEntity.Deliverer,
            InvoiceNumber = receiptEntity.InvoiceNumber,
            Address = warehouse?.address ?? string.Empty,
            MultiPallets = receiptEntity.MultiPallets ?? false,
            ExpectedDeliveryDate = receiptEntity.ExpectedDeliveryDate,
            SharingUrl = sharingUrl ?? "",
            Priority = receiptEntity.Priority,
            Details = [.. detailList.Select(d =>
            {
                skuLookup.TryGetValue(d.SkuId, out var skuInfo);
                bool isException = !d.LocationId.HasValue && string.IsNullOrEmpty(d.PalletCode);

                return new InboundReceiptDetailItemDto
                {
                    Id = d.Id,
                    SkuId = d.SkuId,
                    SkuCode = skuInfo?.Code ?? string.Empty,
                    SkuName = skuInfo?.Name ?? string.Empty,
                    Quantity = d.Quantity,
                    SkuUomId = d.SkuUomId,
                    UnitName = uomLookup.GetValueOrDefault(d.SkuUomId) ?? string.Empty,
                    LocationId = d.LocationId,
                    LocationName = (d.LocationId.HasValue && locationLookup.TryGetValue(d.LocationId.Value, out var locName)) ? locName : string.Empty,
                    PalletCode = d.PalletCode,
                    ExpiryDate = d.ExpiryDate,
                    IsException = isException,
                    SourceNumber = d.SourceNumber,
                };
            })]
        };
    }

    /// <summary>
    /// Create new receipt with details
    /// </summary>
    public async Task<(int id, string message)> CreateAsync(CreateReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        await _actionLogService.AddLogAsync(
            $"Creating new receipt with number {request.ReceiptNo}, Type: {request.Type}, " +
            $"SupplierId: {request.SupplierId}, WarehouseId: {request.WarehouseId}, IsDraft: {request.IsDraft}",
            "Inbound", currentUser);
        return await CreateInternalAsync(request, currentUser, cancellationToken);
    }

    #region Bussiness Logic For Inbound Receipt

    /// <summary>
    /// Update stock
    /// </summary>
    /// <param name="details"></param>  
    /// <param name="receiptNumber"></param>
    /// <param name="supplierId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="availablePallets"></param>
    /// <returns></returns>
    private async Task UpdateStockAsync(List<CreateReceiptDetailDto> details,
        string receiptNumber, int supplierId, CurrentUser currentUser,
        CancellationToken cancellationToken, IEnumerable<AvailablePallet>? availablePallets = null)
    {
        await _actionLogService.AddLogAsync(
            $"[UpdateStock] receipt with number {receiptNumber}, " +
            $"Pallets: {availablePallets?.Count()}" +
            $"SupplierId: {supplierId}, details: {details.Count} rows",
            "Inbound", currentUser);

        var stockDbSet = _dbContext.GetDbSet<StockEntity>();
        var uomIds = details.Select(d => d.SkuUomId).Distinct().ToList();
        var skuIds = details.Select(d => d.SkuId).Distinct().ToList();
        var locationIds = details.Select(d => d.LocationId ?? 0).Distinct().ToList();

        var skuUomLinks = await _dbContext.GetDbSet<SkuUomLinkEntity>()
                                          .Include(s => s.SkuUom)
                                          .Where(link => skuIds.Contains(link.SkuId) && uomIds.Contains(link.SkuUomId))
                                          .ToDictionaryAsync(link => (link.SkuId, link.SkuUomId), cancellationToken);

        var existingStocks = await stockDbSet
            .Where(s => skuIds.Contains(s.sku_id) && locationIds.Contains(s.goods_location_id))
            .ToListAsync(cancellationToken);

        var newStockList = new List<StockEntity>();
        var stockTransactionMappings = new List<(CreateReceiptDetailDto detail,
            StockEntity stock, int conversionRate, int skuOumId, string unitName)>();
        foreach (var item in details)
        {
            decimal baseQty = item.Quantity;
            int currentConversionRate = 1; // default value with 1 

            string currentUnitName = string.Empty;
            if (skuUomLinks.TryGetValue((item.SkuId, item.SkuUomId), out var linkInfo))
            {
                currentConversionRate = linkInfo.ConversionRate;
                currentUnitName = linkInfo.SkuUom?.UnitName ?? string.Empty;

                if (!linkInfo.IsBaseUnit && linkInfo.ConversionRate > 0)
                {
                    baseQty = item.Quantity * linkInfo.ConversionRate;
                }
            }

            var palletCode = item.PalletCode?.Trim() ?? string.Empty;
            var existing = existingStocks.FirstOrDefault(s =>
                s.goods_location_id == item.LocationId &&
                s.sku_id == item.SkuId &&
                s.Palletcode == palletCode &&
                s.SupplierId == supplierId &&
                s.expiry_date.GetValueOrDefault().Date == item.ExpiryDate.GetValueOrDefault().Date);

            if (existing != null)
            {
                existing.actual_qty += baseQty;
                existing.qty += (int)baseQty;
                existing.last_update_time = DateTime.UtcNow;
                stockTransactionMappings.Add((item, existing, currentConversionRate, item.SkuUomId, currentUnitName));
            }
            else
            {
                var newStock = new StockEntity
                {
                    goods_location_id = item.LocationId ?? 0,
                    sku_id = item.SkuId,
                    qty = (int)baseQty,
                    actual_qty = baseQty,
                    Palletcode = palletCode,
                    last_update_time = DateTime.UtcNow,
                    TenantId = currentUser.tenant_id,
                    expiry_date = item.ExpiryDate,
                    goods_owner_id = 1,
                    PutAwayDate = DateTime.UtcNow,
                    SupplierId = supplierId,
                };
                newStockList.Add(newStock);

                stockTransactionMappings.Add((item, newStock, currentConversionRate, item.SkuUomId, currentUnitName));
            }

        }
        if (newStockList.Count > 0)
        {
            await stockDbSet.AddRangeAsync(newStockList, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        var supplierName = await _dbContext.GetDbSet<SupplierEntity>(currentUser.tenant_id)
                                           .Where(s => s.Id == supplierId)
                                           .Select(s => s.supplier_name)
                                           .FirstOrDefaultAsync(cancellationToken);

        var transactionRequests = new List<AddStockTransactionRequest>();
        foreach (var (detail, stock, conversionRate, skuOumId, unitName) in stockTransactionMappings)
        {
            transactionRequests.Add(new AddStockTransactionRequest(
                StockId: stock.Id,
                QuantityChange: detail.Quantity,
                SkuId: detail.SkuId,
                TransactionType: StockTransactionType.Inbound,
                TenantId: currentUser.tenant_id,
                SupplierId: supplierId,
                ConversionRate: conversionRate,
                UnitName: unitName,
                RefReceipt: receiptNumber,
                SupplierName: supplierName,
                SkuUomId: skuOumId
            ));
        }

        await LogStockTransactionAsync(transactionRequests, cancellationToken);
    }

    /// <summary>
    /// Adding Stock Trasaction 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task LogStockTransactionAsync(List<AddStockTransactionRequest> request, CancellationToken cancellationToken)
    {
        if (request is null || request.Count == 0)
        {
            return;
        }
        var transaction = request.Select(r => new StockTransactionEntity
        {
            StockId = r.StockId,
            Quantity = r.QuantityChange,
            SkuId = r.SkuId,
            SkuUomId = r.SkuUomId,
            CurrentConversionRate = r.ConversionRate,
            UnitName = r.UnitName,
            TransactionType = r.TransactionType,
            TenantId = r.TenantId,
            RefReceipt = r.RefReceipt,
            SupplierId = r.SupplierId,
            TransactionDate = DateTime.UtcNow,
            SupplierName = r.SupplierName
        }).ToList();

        var transactionDbSet = _dbContext.GetDbSet<StockTransactionEntity>();
        await transactionDbSet.AddRangeAsync(transaction, cancellationToken);
    }

    /// <summary>
    /// Update Status pallet 
    /// </summary>
    /// <param name="details"></param>
    /// <param name="tenantId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="availablePallets"></param>
    /// <returns></returns>
    private async Task UpdatePalletStatusAsync(List<CreateReceiptDetailDto> details,
        long tenantId, CancellationToken cancellationToken = default,
        IEnumerable<AvailablePallet>? availablePallets = null)
    {
        var palletEntities = _dbContext.GetDbSet<PalletEntity>(tenantId, true);

        var palletCodes = details
            .Where(d => !string.IsNullOrWhiteSpace(d.PalletCode))
            .Select(d => d.PalletCode!.Trim())
            .Distinct()
            .ToList();

        if (availablePallets != null && availablePallets.Any())
        {
            palletCodes = [.. availablePallets.Select(x => x.PalletName).Distinct()];
        }

        if (palletCodes is null)
        {
            _logger.LogWarning("No pallet codes provided in receipt details");
            return;
        }

        var palletsToUpdate = await palletEntities
           .Where(p => palletCodes.Contains(p.PalletCode))
           .ToListAsync(cancellationToken);

        foreach (var pallet in palletsToUpdate)
        {
            pallet.PalletStatus = PalletEnumStatus.InUse;
        }

        _dbContext.GetDbSet<PalletEntity>().UpdateRange(palletsToUpdate);

    }

    /// <summary>
    /// Get existing virtual location for a warehouse, or create one if it doesn't exist.
    /// </summary>
    private async Task<int> GetOrCreateVirtualLocationAsync(int warehouseId,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var locationDbSet = _dbContext.GetDbSet<GoodslocationEntity>();

        var virtualLocation = await locationDbSet
            .FirstOrDefaultAsync(l =>
                l.WarehouseId == warehouseId
                && l.TenantId == currentUser.tenant_id
                && l.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation,
                cancellationToken);

        if (virtualLocation != null)
        {
            return virtualLocation.Id;
        }

        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(currentUser.tenant_id)
                                        .FirstOrDefaultAsync(w => w.Id == warehouseId, cancellationToken);

        var warehouseName = warehouse?.WarehouseName ?? string.Empty;

        var newVirtualLocation = new GoodslocationEntity
        {

            WarehouseId = warehouseId,
            WarehouseName = warehouseName,
            LocationName = string.Empty,
            GoodsLocationType = GoodsLocationTypeEnum.VirtualLocation,
            IsValid = true,
            TenantId = currentUser.tenant_id,
            CreateTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow,
            LocationStatus = (byte)GoodLocationStatusEnum.AVAILABLE,
        };

        await locationDbSet.AddAsync(newVirtualLocation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created virtual location Id={Id} for Warehouse={WarehouseId}, Tenant={TenantId}",
            newVirtualLocation.Id, warehouseId, currentUser.tenant_id);

        return newVirtualLocation.Id;
    }

    /// <summary>
    /// Check the key (sku , supplier ) and mapping when it not exits
    /// </summary>
    /// <param name="supplierId"></param>
    /// <param name="skuIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task EnsureSkuSupplierMappingAsync(
                         int supplierId,
                         List<int> skuIds,
                         CancellationToken cancellationToken)
    {
        if (supplierId <= 0 || skuIds.Count == 0)
        {
            return;
        }

        var mapDbSet = _dbContext.GetDbSet<SkuSupplierEntity>();
        var existingSkuIds = await mapDbSet
                            .Where(x => x.SupplierId == supplierId && skuIds.Contains(x.SkuId))
                            .Select(x => x.SkuId)
                            .ToListAsync(cancellationToken);

        var missingSkuIds = skuIds.Except(existingSkuIds).ToList();
        if (missingSkuIds.Count == 0) return;
        var newMappings = missingSkuIds.Select(skuId => new SkuSupplierEntity
        {
            SkuId = skuId,
            SupplierId = supplierId,
            IsPrimary = false
        });

        await mapDbSet.AddRangeAsync(newMappings, cancellationToken);
    }

    private async Task CreateInboundIntegrationCommandsAsync(
                        InboundReceiptEntity receipt,
                        CreateReceiptRequest request,
                        CurrentUser currentUser,
                        CancellationToken cancellationToken,
                        IEnumerable<AvailablePallet>? availablePallets = null)
    {
        await _actionLogService.AddLogAsync(
            $"[Integration] receipt with number {receipt.ReceiptNumber}, " +
            $"SupplierId: {receipt.SupplierId}, details: {request.Details.Count} rows",
            "Inbound", currentUser);

        var inboundTaskDbSet = _dbContext.GetDbSet<InboundEntity>();
        var historyDbSet = _dbContext.GetDbSet<IntegrationHistory>();
        var now = DateTime.UtcNow;

        var groupedKeys = request.Details
                        .Where(d => !string.IsNullOrWhiteSpace(d.PalletCode) && d.LocationId is > 0)
                        .Select(d => new
                        {
                            PalletCode = d.PalletCode!.Trim(),
                            LocationId = d.LocationId!.Value
                        })
                        .Distinct()
                        .ToList();

        if (groupedKeys.Count == 0) return;
        var palletCodes = groupedKeys.Select(x => x.PalletCode).Distinct().ToList();
        var locationIds = groupedKeys.Select(x => x.LocationId).Distinct().ToList();
        if (availablePallets != null && availablePallets.Any())
        {
            locationIds = availablePallets.Select(x => x.LocationId).Distinct().ToList();
        }

        var existingActive = await inboundTaskDbSet
            .Where(x => x.TenantId == currentUser.tenant_id
                        && x.IsActive
                        && palletCodes.Contains(x.PalletCode)
                        && locationIds.Contains(x.LocationId)
                        && (x.Status == IntegrationStatus.Ready))
            .ToListAsync(cancellationToken);

        var existingSet = existingActive
            .Select(x => $"{x.PalletCode}|{x.LocationId}")
            .ToHashSet();

        var shareTaskCode = GenarationHelper.GenerateTaskCode();

        var newInboundTasks = groupedKeys
        .Where(x => !existingSet.Contains($"{x.PalletCode}|{x.LocationId}"))
        .Select(x => new InboundEntity
        {
            TaskCode = shareTaskCode,
            PalletCode = x.PalletCode,
            LocationId = x.LocationId,
            Priority = request.Priority,
            TenantId = currentUser.tenant_id,
            Status = IntegrationStatus.Ready,
            PickUpDate = request.ExpectedDeliveryDate ?? now,
            CreatedDate = now,
            IsActive = true,
        })
        .ToList();

        if (newInboundTasks.Count > 0)
        {
            await inboundTaskDbSet.AddRangeAsync(newInboundTasks, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var histories = newInboundTasks.Select(task => new IntegrationHistory
            {
                PalletCode = task.PalletCode,
                TaskCode = task.TaskCode,
                HistoryType = HistoryType.Inbound,
                LocationId = task.LocationId,
                Description = $"Task Created from {receipt.ReceiptNumber}",
                TenantId = currentUser.tenant_id,
                Status = task.Status,
                CreatedDate = task.CreatedDate,
                Priority = task.Priority,
                IsActive = task.IsActive,
            }).ToList();

            await historyDbSet.AddRangeAsync(histories, cancellationToken);
        }

        var allRelevantTasks = await inboundTaskDbSet
            .Where(x => x.TenantId == currentUser.tenant_id
                        && x.Status == IntegrationStatus.Ready
                        && x.IsActive
                        && palletCodes.Contains(x.PalletCode)
                        && locationIds.Contains(x.LocationId))
            .Select(x => new { x.Id, x.PalletCode, x.LocationId })
            .ToListAsync(cancellationToken);

        var taskLookup = allRelevantTasks
                      .GroupBy(x => new
                      {
                          PalletCode = x.PalletCode.Trim(),
                          x.LocationId
                      })
                      .ToDictionary(
                          g => g.Key,
                          g => g.OrderByDescending(x => x.Id).First().Id
                      );

        var mappingDbSet = _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>();
        var mappingsToInsert = new List<ReceiptDetailInboundIntegrationEntity>();

        if (receipt.Details != null && receipt.Details.Count != 0)
        {
            foreach (var detail in receipt.Details)
            {
                if (string.IsNullOrWhiteSpace(detail.PalletCode) || detail.LocationId is not > 0)
                    continue;

                var key = new
                {
                    PalletCode = detail.PalletCode.Trim(),
                    LocationId = detail.LocationId.Value
                };

                if (taskLookup.TryGetValue(key, out var taskId))
                {
                    mappingsToInsert.Add(new ReceiptDetailInboundIntegrationEntity
                    {
                        ReceiptDetailId = detail.Id,
                        InboundId = (int)taskId,
                        CreateDate = now
                    });
                }
            }
        }

        if (mappingsToInsert.Count > 0)
        {
            await mappingDbSet.AddRangeAsync(mappingsToInsert, cancellationToken);
        }
    }


    #endregion bussiness logic for receipt

    /// <summary>
    /// Get next receipt number
    /// </summary>
    /// <returns>next receipt number</returns>
    public async Task<string> GetNextReceiptNoAsync()
    {
        var tableName = "inbound_receipt";
        var prefixEntity = await _dbContext.GetDbSet<GlobalUniqueSerialEntity>()
                                           .FirstOrDefaultAsync(x => x.table_name == tableName);

        var receiptNo = await _functionHelper.GetFormNoAsync(tableName, prefixEntity?.prefix_char ?? "PNK-");
        return receiptNo;
    }

    /// <summary>
    /// Asynchronously updates the receipt identified by the specified ID using the provided request data.
    /// </summary>
    /// <param name="id">The unique identifier of the receipt to update. Must correspond to an existing receipt.</param>
    /// <param name="request">The data used to update the receipt, including all required fields for processing.</param>
    /// <param name="currentUser">The user performing the update operation. Used for authorization and auditing purposes.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the update operation.</param>
    /// <returns>A task representing the asynchronous update operation. The task result contains a string indicating the outcome
    /// of the update.</returns>
    public async Task<(bool, string)> UpdateAsync(int id, UpdateInboundReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {

        if (request.Details == null || request.Details.Count == 0)
        {
            return (false, _localizer["Details are required"]);
        }
        if (request.WarehouseId <= 0)
        {
            return (false, _localizer["Warehouse is required"]);
        }

        var receiptDbSet = _dbContext.GetDbSet<InboundReceiptEntity>();
        var existingReceipt = await receiptDbSet
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == currentUser.tenant_id, cancellationToken);

        if (existingReceipt is null)
        {
            return (false, _localizer["Receipt not found"]);
        }

        if (existingReceipt.Status != ReceiptStatus.DRAFT && existingReceipt.Status != ReceiptStatus.NEW)
        {
            return (false, _localizer["Cannot update receipt in current status. Only DRAFT or NEW allowed."]);
        }

        var availablePallets = new List<InboundPalletEntity>();
        if (request.MultiPallets)
        {
            availablePallets = await _dbContext.GetDbSet<InboundPalletEntity>()
                .Where(x => x.ReceiptId == id)
                .ToListAsync(cancellationToken);

            if (availablePallets.Count < 1) return (false, _localizer["Receipt details not found"]);
        }

        await _actionLogService.AddLogAsync(
          $"[Update] receipt ReceiptNo {request.ReceiptNo} IsStored: {request.IsStored} Details {request.Details.Count} rows",
          "Inbound", currentUser);

        if (request.IsStored)
        {
            var invalidDetail = request.Details
                     .FirstOrDefault(d => d.LocationId is null or <= 0);
            if (invalidDetail != null)
            {
                return (false, _localizer["Location is required when storing"]);
            }

            var requestConflict = request.Details
                                       .Where(d => !string.IsNullOrWhiteSpace(d.PalletCode))
                                       .Select(d => new
                                       {
                                           PalletCode = d.PalletCode!.Trim(),
                                           LocationId = d.LocationId!.Value
                                       })
                                       .GroupBy(x => x.PalletCode)
                                       .FirstOrDefault(g => g.Select(x => x.LocationId).Distinct().Count() > 1);

            if (requestConflict != null)
            {
                return (false, _localizer["One pallet cannot belong to multiple locations in the same receipt"]);
            }
        }
        else
        {
            var virtualLocationId = await GetOrCreateVirtualLocationAsync(
                request.WarehouseId, currentUser, cancellationToken);

            if (virtualLocationId <= 0)
            {
                _logger.LogError("Failed to get or create virtual location for WarehouseId={WarehouseId}, TenantId={TenantId}",
                    request.WarehouseId, currentUser.tenant_id);
                return (false, _localizer["Failed to resolve virtual location"]);
            }

            foreach (var details in request.Details)
            {
                details.LocationId = virtualLocationId;
                details.PalletCode = null;
            }
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        existingReceipt.WarehouseId = request.WarehouseId;
        existingReceipt.SupplierId = request.SupplierId;
        existingReceipt.Type = request.Type;
        existingReceipt.Description = request.Description;
        existingReceipt.LastUpdateTime = DateTime.UtcNow;
        existingReceipt.IsStored = request.IsStored;
        existingReceipt.Deliverer = request.Deliverer;
        existingReceipt.InvoiceNumber = request.InvoiceNumber;
        existingReceipt.ExpectedDeliveryDate = request.ExpectedDeliveryDate;

        var isTransitioningToNew = false;
        if (request.IsUpgradeStatus && existingReceipt.Status == ReceiptStatus.DRAFT)
        {
            existingReceipt.Status = ReceiptStatus.NEW;
            isTransitioningToNew = true;
        }

        await EnsureSkuSupplierMappingAsync(request.SupplierId, [.. request.Details.Select(d => d.SkuId).Distinct()], cancellationToken);

        var mappingDbSet = _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>();
        var inboundTaskDbSet = _dbContext.GetDbSet<InboundEntity>();

        var currentDetailIds = existingReceipt.Details.Select(d => d.Id).ToList();
        var existingMappings = await mappingDbSet
            .Where(m => currentDetailIds.Contains(m.ReceiptDetailId))
            .ToListAsync(cancellationToken);

        var processedDetailIds = new HashSet<int>();
        var matchedExistingDetails = new List<InboundReceiptDetailEntity>();
        var newlyAddedDetailsForWcs = new List<CreateReceiptDetailDto>();
        var detailDbSet = _dbContext.GetDbSet<InboundReceiptDetailEntity>();

        foreach (var incomingItem in request.Details)
        {
            if (incomingItem.SkuId <= 0 || incomingItem.Quantity <= 0 || incomingItem.SkuUomId <= 0)
            {
                return (false, _localizer["Invalid detail item"]);
            }

            if (incomingItem.Id > 0)
            {
                var existingDetail = existingReceipt.Details.FirstOrDefault(d => d.Id == incomingItem.Id);

                if (existingDetail != null)
                {
                    existingDetail.Quantity = incomingItem.Quantity;
                    existingDetail.ExpiryDate = incomingItem.ExpiryDate?.Date;
                    existingDetail.SkuUomId = incomingItem.SkuUomId;
                    existingDetail.LocationId = request.MultiPallets ? 0 : incomingItem.LocationId;
                    existingDetail.PalletCode = incomingItem.PalletCode;
                    existingDetail.SourceNumber = incomingItem.SourceNumber;

                    processedDetailIds.Add(existingDetail.Id);
                }
                else
                {
                    return (false, _localizer[$"Detail with ID {incomingItem.Id} not found in this receipt."]);
                }
            }

            else
            {
                existingReceipt.Details.Add(new InboundReceiptDetailEntity
                {
                    ReceiptId = existingReceipt.Id,
                    SkuId = incomingItem.SkuId,
                    Quantity = incomingItem.Quantity,
                    SkuUomId = incomingItem.SkuUomId,
                    CreateDate = DateTime.UtcNow,
                    ExpiryDate = incomingItem.ExpiryDate?.Date,
                    LocationId = incomingItem.LocationId,
                    PalletCode = incomingItem.PalletCode,
                    SourceNumber = incomingItem.SourceNumber,
                });

                if (!request.MultiPallets)
                {
                    newlyAddedDetailsForWcs.Add(new CreateReceiptDetailDto
                    {
                        SkuId = incomingItem.SkuId,
                        Quantity = incomingItem.Quantity,
                        SkuUomId = incomingItem.SkuUomId,
                        ExpiryDate = incomingItem.ExpiryDate?.Date,
                        LocationId = incomingItem.LocationId,
                        PalletCode = incomingItem.PalletCode,
                        AsnId = incomingItem.AsnId
                    });
                }
            }
        }

        foreach (var item in availablePallets)
        {
            var incomingItem = request.Details.FirstOrDefault(d => d.SkuId == item.SkuId);
            newlyAddedDetailsForWcs.Add(new CreateReceiptDetailDto
            {
                SkuId = item.SkuId,
                Quantity = item.Quantity,
                SkuUomId = item.SkuUomId,
                ExpiryDate = incomingItem?.ExpiryDate?.Date,
                LocationId = item.LocationId,
                PalletCode = item.PalletCode,
                AsnId = incomingItem?.AsnId
            });
        }

        var detailsToRemove = existingReceipt.Details
                            .Where(d => d.Id != 0 && !processedDetailIds.Contains(d.Id))
                            .ToList();

        if (detailsToRemove.Count != 0)
        {
            var removedDetailIds = detailsToRemove.Select(d => d.Id).ToList();
            var mappingsToRemove = existingMappings
                                   .Where(m => removedDetailIds.Contains(m.ReceiptDetailId))
                                   .ToList();

            if (mappingsToRemove.Count != 0)
            {
                var taskIdsToCancel = mappingsToRemove.Select(m => m.InboundId).Distinct().ToList();
                var linkedTasks = await inboundTaskDbSet
                    .Where(t => taskIdsToCancel.Contains((int)t.Id))
                    .ToListAsync(cancellationToken);

                foreach (var task in linkedTasks)
                {
                    if (task.Status == IntegrationStatus.Processing)
                    {
                        return (false, _localizer[$"Cannot update/remove details. Pallet {task.PalletCode} is currently being processed by WCS."]);
                    }
                    // update task is not avtive 
                    task.IsActive = false;
                    //  task.Status = IntegrationStatus.Cancelled; 
                }
                mappingDbSet.RemoveRange(mappingsToRemove);
            }
            detailDbSet.RemoveRange(detailsToRemove);
        }

        // Get the list of active details after processing updates and removals
        var activeDetails = existingReceipt.Details
                                           .Where(d => d.Id == 0 || processedDetailIds.Contains(d.Id))
                                           .ToList();

        if (isTransitioningToNew)
        {
            if (request.IsStored)
            {
                var allDetailsForWcs = existingReceipt.Details.Select(d => new CreateReceiptDetailDto
                {
                    SkuId = d.SkuId,
                    Quantity = d.Quantity,
                    SkuUomId = d.SkuUomId,
                    ExpiryDate = d.ExpiryDate,
                    LocationId = d.LocationId,
                    PalletCode = d.PalletCode
                }).ToList();

                var createIntegration = new CreateReceiptRequest
                {
                    SupplierId = request.SupplierId,
                    WarehouseId = request.WarehouseId,
                    CreatedDate = existingReceipt.CreateDate,
                    Description = request.Description,
                    IsStored = request.IsStored,
                    ReceiptNo = existingReceipt.ReceiptNumber,
                    Type = request.Type,
                    Details = request.MultiPallets ? newlyAddedDetailsForWcs : allDetailsForWcs
                };

                await CreateInboundIntegrationCommandsAsync(existingReceipt, createIntegration, currentUser, cancellationToken);
                await UpdatePalletStatusForUpdateMethodAsync(activeDetails, currentUser, cancellationToken);
            }
            else
            {
                existingReceipt.Status = ReceiptStatus.COMPLETE;
                await UpdateStockForUpdateMethodAsync(activeDetails, request.SupplierId, existingReceipt.ReceiptNumber, currentUser, cancellationToken);
            }
        }

        else if (existingReceipt.Status == ReceiptStatus.NEW && newlyAddedDetailsForWcs.Count != 0)
        {

            var createIntegration = new CreateReceiptRequest
            {
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                CreatedDate = existingReceipt.CreateDate,
                Description = request.Description,
                IsStored = request.IsStored,
                ReceiptNo = existingReceipt.ReceiptNumber,
                Type = request.Type,
                Details = newlyAddedDetailsForWcs
            };

            await CreateInboundIntegrationCommandsAsync(existingReceipt, createIntegration, currentUser, cancellationToken);

            var newEntitiesForPalletUpdate = existingReceipt.Details.Where(d => newlyAddedDetailsForWcs.Select(n => n.PalletCode).Contains(d.PalletCode)).ToList();
            await UpdatePalletStatusForUpdateMethodAsync(newEntitiesForPalletUpdate, currentUser, cancellationToken);
        }
        else
        {
            if (request.IsStored)
            {
                await UpdatePalletStatusForUpdateMethodAsync([.. existingReceipt.Details], currentUser, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (true, _localizer["update_success"]);
    }

    #region bussiness case for update

    private async Task UpdatePalletStatusForUpdateMethodAsync(List<InboundReceiptDetailEntity> details, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var palletEntities = _dbContext.GetDbSet<PalletEntity>(currentUser.tenant_id);
        var palletCodes = details
            .Where(d => !string.IsNullOrWhiteSpace(d.PalletCode))
            .Select(d => d.PalletCode!.Trim())
            .Distinct()
            .ToList();

        if (palletCodes is null)
        {
            _logger.LogWarning("No pallet codes provided in receipt details");
            return;
        }

        var palletsToUpdate = await palletEntities
           .Where(p => palletCodes.Contains(p.PalletCode))
           .ToListAsync(cancellationToken);

        foreach (var pallet in palletsToUpdate)
        {
            pallet.PalletStatus = PalletEnumStatus.InUse;
        }

        _dbContext.GetDbSet<PalletEntity>().UpdateRange(palletsToUpdate);

    }

    private async Task UpdateStockForUpdateMethodAsync(List<InboundReceiptDetailEntity> details, int supplierId, string receiptNumber, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var stockDbSet = _dbContext.GetDbSet<StockEntity>();

        var uomIds = details.Select(d => d.SkuUomId).Distinct().ToList();
        var skuIds = details.Select(d => d.SkuId).Distinct().ToList();
        var locationIds = details.Select(d => d.LocationId ?? 0).Distinct().ToList();


        var skuUomLinks = await _dbContext.GetDbSet<SkuUomLinkEntity>()
                                          .Include(s => s.SkuUom)
                                          .Where(link => skuIds.Contains(link.SkuId) && uomIds.Contains(link.SkuUomId))
                                          .ToDictionaryAsync(link => (link.SkuId, link.SkuUomId), cancellationToken);

        var existingStocks = await stockDbSet
            .Where(s => skuIds.Contains(s.sku_id) && locationIds.Contains(s.goods_location_id))
            .ToListAsync(cancellationToken);

        var stockTransactionMappings = new List<(InboundReceiptDetailEntity detail, StockEntity stock, int conversionRate, int skuOumId, string unitName)>();
        var newStockList = new List<StockEntity>();

        foreach (var item in details)
        {
            decimal baseQty = item.Quantity;
            int currentConversionRate = 1; // default value with 1 

            string currentUnitName = string.Empty;

            if (skuUomLinks.TryGetValue((item.SkuId, item.SkuUomId), out var linkInfo))
            {
                currentConversionRate = linkInfo.ConversionRate;
                currentUnitName = linkInfo.SkuUom?.UnitName ?? string.Empty;

                if (!linkInfo.IsBaseUnit && linkInfo.ConversionRate > 0)
                {
                    baseQty = item.Quantity * linkInfo.ConversionRate;
                }
            }

            var palletCode = item.PalletCode?.Trim() ?? string.Empty;

            var existing = existingStocks.FirstOrDefault(s =>
                s.goods_location_id == item.LocationId &&
                s.sku_id == item.SkuId &&
                s.Palletcode == palletCode &&
                s.expiry_date.GetValueOrDefault().Date == item.ExpiryDate.GetValueOrDefault().Date);

            if (existing != null)
            {
                existing.actual_qty += baseQty;
                existing.qty += (int)baseQty;
                existing.last_update_time = DateTime.UtcNow;
                stockTransactionMappings.Add((item, existing, currentConversionRate, item.SkuUomId, currentUnitName));
            }
            else
            {
                var newStock = new StockEntity
                {
                    goods_location_id = item.LocationId ?? 0,
                    sku_id = item.SkuId,
                    qty = (int)baseQty,
                    actual_qty = baseQty,
                    Palletcode = palletCode,
                    last_update_time = DateTime.UtcNow,
                    TenantId = currentUser.tenant_id,
                    expiry_date = item.ExpiryDate,
                    goods_owner_id = 1,
                    PutAwayDate = DateTime.UtcNow,
                    SupplierId = supplierId,
                };
                newStockList.Add(newStock);

                stockTransactionMappings.Add((item, newStock, currentConversionRate, item.SkuUomId, currentUnitName));
            }

        }
        if (newStockList.Count > 0)
        {
            await stockDbSet.AddRangeAsync(newStockList, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        var supplierName = await _dbContext.GetDbSet<SupplierEntity>(currentUser.tenant_id)
                                           .Where(s => s.Id == supplierId)
                                           .Select(s => s.supplier_name)
                                           .FirstOrDefaultAsync(cancellationToken);

        var transactionRequests = new List<AddStockTransactionRequest>();
        foreach (var mapping in stockTransactionMappings)
        {
            transactionRequests.Add(new AddStockTransactionRequest(
                StockId: mapping.stock.Id,
                QuantityChange: mapping.detail.Quantity,
                SkuId: mapping.detail.SkuId,
                TransactionType: StockTransactionType.Inbound,
                TenantId: currentUser.tenant_id,
                SupplierId: supplierId,
                ConversionRate: mapping.conversionRate,
                UnitName: mapping.unitName,
                RefReceipt: receiptNumber,
                SupplierName: supplierName,
                SkuUomId: mapping.skuOumId
            ));
        }

        await LogStockTransactionAsync(transactionRequests, cancellationToken);
    }
    #endregion

    /// <summary>
    /// Cancel async
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> CancelAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var receipt = await _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id, isTracking: true)
                                     .Include(x => x.Details)
                                     .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (receipt is null)
        {
            return (false, _localizer["Receipt not found"]);
        }

        if (receipt.Status == ReceiptStatus.PROCESSING || receipt.Status == ReceiptStatus.CANCELED)
        {
            return (false, _localizer["Receipt is already processing or cancel , can't not do"]);
        }

        if (receipt.Details != null && receipt.Details.Count != 0)
        {
            var detailIds = receipt.Details.Select(d => d.Id).ToList();

            var taskIds = await _dbContext.GetDbSet<ReceiptDetailInboundIntegrationEntity>()
                .Where(m => detailIds.Contains(m.ReceiptDetailId))
                .Select(m => m.InboundId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (taskIds.Count != 0)
            {
                var linkedTasks = await _dbContext.GetDbSet<InboundEntity>()
                                    .Where(t => taskIds.Contains((int)t.Id))
                                    .ToListAsync(cancellationToken);

                var processingTask = linkedTasks.FirstOrDefault(t => t.Status == IntegrationStatus.Processing);
                if (processingTask != null)
                {
                    return (false, _localizer[$"Cannot cancel receipt. Pallet {processingTask.PalletCode} is currently being processed by WCS."]);
                }

                foreach (var task in linkedTasks)
                {
                    task.IsActive = false;
                    // task.Status = IntegrationStatus.Cancelled;
                }
            }
        }

        receipt.Status = ReceiptStatus.CANCELED;
        receipt.LastUpdateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, _localizer["Receipt cancelled successfully"]);
    }

    /// <summary>
    /// Retry task
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool, string)> RetryInboundDetailsAsync(RetryInboundRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var detailIds = request.Items.Select(i => i.DetailId).ToList();
        var receipt = await _dbContext.GetDbSet<InboundReceiptEntity>()
                                      .Include(r => r.Details.Where(d => detailIds.Contains(d.Id)))
                                      .FirstOrDefaultAsync(r => r.Id == request.ReceiptId, cancellationToken);

        if (receipt is null)
        {
            _logger.LogError("Receipt with ID {ReceiptId} not found for retry", request.ReceiptId);
            return (false, _localizer["Receipt not found"]);
        }

        var retryItemsForWcs = new List<CreateReceiptDetailDto>();

        receipt.LastUpdateTime = DateTime.UtcNow;
        foreach (var retryInfo in request.Items)
        {
            var detail = receipt.Details.FirstOrDefault(d => d.Id == retryInfo.DetailId);
            if (detail == null) continue;

            detail.LocationId = retryInfo.LocationId;
            detail.PalletCode = retryInfo.PalletCode;

            retryItemsForWcs.Add(new CreateReceiptDetailDto
            {
                SkuId = detail.SkuId,
                Quantity = detail.Quantity,
                SkuUomId = detail.SkuUomId,
                ExpiryDate = detail.ExpiryDate,
                LocationId = detail.LocationId,
                PalletCode = detail.PalletCode
            });
        }

        if (retryItemsForWcs.Count == 0)
        {
            return (false, _localizer["No valid details to retry"]);
        }

        var integrationRequest = new CreateReceiptRequest
        {
            SupplierId = receipt.SupplierId,
            WarehouseId = receipt.WarehouseId,
            ReceiptNo = receipt.ReceiptNumber,
            IsStored = true,
            Details = retryItemsForWcs
        };

        await CreateInboundIntegrationCommandsAsync(receipt, integrationRequest, currentUser, cancellationToken);
        await UpdatePalletStatusAsync(retryItemsForWcs, currentUser.tenant_id, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (true, _localizer["Retry successful"]);
    }

    /// <summary>
    /// Dashboard get Inbound
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<OrderItemDTO>> GetInboundInfo(CurrentUser currentUser)
    {
        var query = from receipt in _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id)
                    group receipt by receipt.Status into g
                    select new OrderItemDTO
                    {
                        ItemStatus = g.Key,
                        TotalCount = g.Count()
                    };

        return await query.ToListAsync();
    }

    /// <summary>
    /// Get Inbound By Date
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="finishStatuses"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<OrderItemDTO>> GetInboundByDate(CurrentUser currentUser, ReceiptStatus[] finishStatuses, DateTime dateTime)
    {
        var query = from receipt in _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id)
                    where finishStatuses.Contains(receipt.Status) && receipt.LastUpdateTime.Date == dateTime.Date
                    group receipt by receipt.Status into g
                    select new OrderItemDTO
                    {
                        ItemStatus = g.Key,
                        TotalCount = g.Count()
                    };

        return await query.ToListAsync();
    }

    /// <summary>
    /// Inbound By RangeDate
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="finishStatuses"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<DateOrderItemDTO>> GetInboundByRangeDate(CurrentUser currentUser,
        ReceiptStatus[] finishStatuses, DateTime fromDate, DateTime toDate)
    {
        var query = from receipt in _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id)
                    where finishStatuses.Contains(receipt.Status)
                        && receipt.LastUpdateTime.Date >= fromDate.Date
                        && receipt.LastUpdateTime.Date <= toDate.Date
                    group receipt by receipt.LastUpdateTime.Date into g
                    select new DateOrderItemDTO
                    {
                        Date = g.Key,          // the grouped date
                        TotalCount = g.Count() // number of receipts on that date
                    };

        return await query.ToListAsync();
    }

    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> ImportExcelData(List<InboundOrderExcel> requests,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        await _actionLogService.AddLogAsync(
           $"[ImportExcel] receipt details: {requests.Count} rows",
           "Inbound", currentUser);

        var tenantId = currentUser.tenant_id;
        string userName = currentUser.user_name;
        var skus = requests.Select(x => x.SkuCode.Trim()).Distinct().ToList();
        var units = requests.Select(x => x.UnitName.Trim()).Distinct().ToList();
        var wareHouses = requests.Select(x => x.WareHouseName.Trim()).Distinct().ToList();
        var suppliers = requests.Select(x => x.SupplierName.Trim()).Distinct().ToList();

        var locations = requests.Select(x => x.LocationCode?.Trim()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

        ProccesingAddMatterData(tenantId, units, wareHouses, suppliers, skus, userName);
        await _dbContext.SaveChangesAsync(cancellationToken);

        EnsureStockHasVirutalLocations(tenantId, wareHouses);
        await _dbContext.SaveChangesAsync(cancellationToken);

        int index = 0;
        int insertedCount = 0;
        int totalRequests = requests.Count;
        var queryReceipt = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId, true);
        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var supplierList = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        //var orders = requests.Select(x => x.OrderCode.Trim()).Distinct().ToList();
        var uniqueOrders = requests.Select(x => new
        {
            Code = x.OrderCode.Trim(),
            WareHouseName = x.WareHouseName.Trim(),
            IsPutaway = x.IsPutaway?.Trim().ToLower() == "yes" || x.IsPutaway?.Trim().ToLower() == "true",
            SupplierName = x.SupplierName.Trim()
        }).GroupBy(x => new { x.Code, x.WareHouseName, x.SupplierName, x.IsPutaway })
        .Select(g => g.First());

        do
        {
            var batchOrders = uniqueOrders.Skip(index).Take(SystemDefine.BatchSize).ToList();
            var listPutaway = new List<InboundReceiptEntity>();
            var items = new List<InboundReceiptEntity>();

            foreach (var order in batchOrders)
            {
                var wareHouse = wareHouseList.FirstOrDefault(w => w.WarehouseName == order.WareHouseName)
                    ?? throw new Exception($"Warehouse not found for name: {order.WareHouseName}");
                var supplierId = GetFirstSupplierId(supplierList, order.SupplierName);
                var inboundReceiptEntity = MapInboundReceiptEntity(order.IsPutaway, supplierId, wareHouse,
                    order.Code, requests, tenantId, userName, skuList, uomList, locationList);
                items.Add(inboundReceiptEntity);
                if (order.IsPutaway)
                {
                    listPutaway.Add(inboundReceiptEntity);
                }
            }

            try
            {
                // save Draft
                var res = await SaveNewOrdersAsync(items, queryReceipt);
                insertedCount += res;

                var detailsForPalletUpdate = new List<CreateReceiptDetailDto>();
                foreach (var receipt in listPutaway)
                {
                    detailsForPalletUpdate.AddRange(receipt.Details.Select(d => new CreateReceiptDetailDto
                    {
                        SkuId = d.SkuId,
                        Quantity = d.Quantity,
                        SkuUomId = d.SkuUomId,
                        ExpiryDate = d.ExpiryDate,
                        LocationId = d.LocationId,
                        PalletCode = d.PalletCode
                    }));
                }
                await UpdatePalletStatusAsync(detailsForPalletUpdate, tenantId, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {Index}", index);
                // Optionally, you can choose to break the loop or continue with the next batch
                // break;
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalRequests);

        return insertedCount;
    }

    private void EnsureStockHasVirutalLocations(long tenantId, List<string> wareHouses)
    {
        foreach (var wareHouseName in wareHouses)
        {
            var wareHouse = _dbContext.GetDbSet<WarehouseEntity>(tenantId)
                .FirstOrDefault(w => w.WarehouseName == wareHouseName);
            if (wareHouse == null)
            {
                _logger.LogWarning("Warehouse not found for name: {warehouseName}", wareHouseName);
                continue;
            }
            var existingVirtualLocation = _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                                                   .FirstOrDefault(l => l.WarehouseId == wareHouse.Id &&
                                                   l.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation);
            if (existingVirtualLocation == null)
            {
                _dbContext.GetDbSet<GoodslocationEntity>().Add(new GoodslocationEntity
                {
                    LocationName = $"Virtual Location for {wareHouseName}",
                    WarehouseId = wareHouse.Id,
                    TenantId = tenantId,
                    WarehouseName = $"VirtualLocation_{wareHouse.WarehouseName}",
                    GoodsLocationType = GoodsLocationTypeEnum.VirtualLocation,
                    CreateTime = DateTime.UtcNow,
                    LastUpdateTime = DateTime.UtcNow,
                    IsValid = true
                });
            }
        }
    }

    private async Task<int> SaveNewOrdersAsync(List<InboundReceiptEntity> items,
        IQueryable<InboundReceiptEntity> queryReceipt)
    {
        var newList = new List<InboundReceiptEntity>();
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.ReceiptNumber)) continue;

            var receipt = await queryReceipt.FirstOrDefaultAsync(r => r.ReceiptNumber == item.ReceiptNumber);
            if (receipt != null) continue;
            //TODO fix duplicate receipt number case, currently just skip, can consider add suffix to make it unique or update existing one

            _dbContext.GetDbSet<InboundReceiptEntity>().Add(item);
        }

        return await _dbContext.SaveChangesAsync();
    }

    private InboundReceiptEntity MapInboundReceiptEntity(bool isPutaway, int supplierId, WarehouseEntity wareHouse,
        string orderNo, List<InboundOrderExcel> requests, long tenantId, string userName, IQueryable<SkuEntity> skuList,
        IQueryable<SkuUomEntity> uomList, IQueryable<GoodslocationEntity> locationList)
    {
        var items = requests.Where(x => x.OrderCode == orderNo).ToList();
        var orderType = items.FirstOrDefault()?.OrderType ?? "ExcelImport";
        var note = items.FirstOrDefault()?.Note ?? $"Imported from Excel for order {orderNo}";

        var mylocations = locationList.Where(x => x.WarehouseId == wareHouse.Id);
        return new InboundReceiptEntity
        {
            Status = ReceiptStatus.DRAFT,
            TenantId = tenantId,
            ReceiptNumber = orderNo,
            Type = orderType,
            IsStored = isPutaway,
            WarehouseId = wareHouse.Id, // You may want to set this based on the wareHouses list
            SupplierId = supplierId, // You may want to set this based on the suppliers list
            Description = note,
            Creator = userName ?? "System",
            CreateDate = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow,
            Details = GetDetails(wareHouse, items, tenantId, skuList, uomList, mylocations)
        };
    }

    private int GetFirstSupplierId(IQueryable<SupplierEntity> supplierList, string supplierName)
    {
        return supplierList.FirstOrDefault(x => x.supplier_code == supplierName
            || x.supplier_name == supplierName)?.Id ?? 0;
    }

    private List<InboundReceiptDetailEntity> GetDetails(WarehouseEntity wareHouse,
        List<InboundOrderExcel> items, long tenantId,
        IQueryable<SkuEntity> skuList, IQueryable<SkuUomEntity> uomList,
        IQueryable<GoodslocationEntity> locationList)
    {
        var details = new List<InboundReceiptDetailEntity>();
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.SkuCode) || string.IsNullOrEmpty(item.UnitName) || string.IsNullOrEmpty(item.Qty))
            {
                _logger.LogWarning("Skipping item with missing required fields: SkuCode={SkuCode}, UnitName={UnitName}, Qty={Qty}",
                    item.SkuCode, item.UnitName, item.Qty);
                continue;
            }

            var sku = skuList.FirstOrDefault(x => x.sku_code == item.SkuCode || x.sku_name == item.SkuCode);
            var uom = uomList.FirstOrDefault(x => x.UnitName == item.UnitName);
            var locationId = GetLocationId(locationList, item.LocationCode, wareHouse, tenantId);

            // You may want to set this based on the LocationCode
            details.Add(new InboundReceiptDetailEntity
            {
                SkuId = sku?.Id ?? 0, // You may want to set this based on the SkuCode
                Quantity = decimal.TryParse(item.Qty, out var qty) ? qty : 0,
                SkuUomId = uom?.Id ?? 0, // You may want to set this based on the UnitName
                ExpiryDate = item.ExpireDate,
                LocationId = locationId, // Location can be assigned later
                PalletCode = $"PAL-{GenarationHelper.GetRandomPassword(6)}" // Pallet code can be assigned later
            });
        }

        return details;
    }

    private int? GetLocationId(IQueryable<GoodslocationEntity> locationList, string locationCode,
        WarehouseEntity wareHouse, long tenantId)
    {
        return locationList
            .FirstOrDefault(x => x.LocationName == locationCode && x.WarehouseId == wareHouse.Id)?.Id
             ?? GetVirtualLocation(locationList, wareHouse, tenantId);
    }

    private int? GetVirtualLocation(IQueryable<GoodslocationEntity> locationList, WarehouseEntity wareHouse, long tenantId)
    {
        var rs = locationList.FirstOrDefault(x => x.WarehouseId == wareHouse.Id
            && x.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation);
        if (rs == null)
        {
            _logger.LogWarning("Virtual location not found for WarehouseId={WarehouseId}", wareHouse.Id);
            _dbContext.GetDbSet<GoodslocationEntity>().Add(new GoodslocationEntity
            {
                WarehouseId = wareHouse.Id,
                WarehouseName = $"VirtualLocation_{wareHouse.WarehouseName}",
                LocationName = $"VirtualLocation_{wareHouse.Id}",
                GoodsLocationType = GoodsLocationTypeEnum.VirtualLocation,
                TenantId = tenantId,
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow,
                IsValid = true
            });

            _dbContext.SaveChanges();

            rs = locationList.FirstOrDefault(x => x.WarehouseId == wareHouse.Id && x.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation);

            if (rs == null) return null;
        }

        return rs?.Id;
    }

    private void ProccesingAddMatterData(long tenantId, List<string> units,
        List<string> wareHouses, List<string> suppliers, List<string> skus, string userName)
    {
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        foreach (var sku in skus)
        {
            if (string.IsNullOrEmpty(sku)) continue;

            var hasData = skuList.Any(s => s.sku_code == sku || s.sku_name == sku);
            if (hasData) continue;

            _dbContext.GetDbSet<SkuEntity>().Add(new SkuEntity
            {
                sku_code = $"SKU-{GenarationHelper.GetRandomPassword(6)}",
                sku_name = sku,
                create_time = DateTime.UtcNow,
                last_update_time = DateTime.UtcNow,
                TenantId = tenantId,
            });
        }

        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        foreach (var item in units)
        {
            if (string.IsNullOrEmpty(item)) continue;

            var hasData = uomList.Any(s => s.UnitName == item);
            if (hasData) continue;

            _dbContext.GetDbSet<SkuUomEntity>().Add(new SkuUomEntity
            {
                UnitName = item,
                TenantId = tenantId,
                Description = $"Auto added unit {item} by {userName} at {DateTime.UtcNow}"
            });
        }

        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        foreach (var stock in wareHouses)
        {
            if (string.IsNullOrEmpty(stock)) continue;

            var hasData = wareHouseList.Any(s => s.WarehouseName == stock || s.WcsBlockId == stock);
            if (hasData) continue;

            _dbContext.GetDbSet<WarehouseEntity>().Add(new WarehouseEntity
            {
                WarehouseName = stock,
                creator = userName,
                last_update_time = DateTime.UtcNow,
                create_time = DateTime.UtcNow,
                is_valid = true,
                TenantId = tenantId,
            });
        }

        var supplierList = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        foreach (var supplier in suppliers)
        {
            if (string.IsNullOrEmpty(supplier)) continue;

            var hasData = supplierList.Any(s => s.supplier_code == supplier || s.supplier_name == supplier);
            if (hasData) continue;

            _dbContext.GetDbSet<SupplierEntity>().Add(new SupplierEntity
            {
                supplier_name = supplier,
                supplier_code = $"SUP-{GenarationHelper.GetRandomPassword(6)}",
                TenantId = tenantId,
                create_time = DateTime.UtcNow,
                last_update_time = DateTime.UtcNow,
                creator = userName,
                is_valid = true,
            });
        }
    }

    /// <summary>
    /// Revert Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> RevertInbound(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var receipt = await _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id, true)
                                     .FirstOrDefaultAsync(x => x.Id == id && x.Status == ReceiptStatus.CANCELED, cancellationToken);

        if (receipt is null)
        {
            return (false, _localizer["Receipt not found"]);
        }

        receipt.Status = ReceiptStatus.DRAFT;
        receipt.LastUpdateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, _localizer["Receipt Reverting successfully"]);
    }

    /// <summary>
    /// Clone Inbound 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool success, string message)> CloneInboundAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var templateReceipt = await _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
                                     .Include(x => x.Details)
                                     .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (templateReceipt is null)
        {
            return (false, _localizer["Receipt not found"]);
        }

        templateReceipt.Status = ReceiptStatus.DRAFT;
        templateReceipt.CreateDate = DateTime.UtcNow;
        templateReceipt.LastUpdateTime = DateTime.UtcNow;
        templateReceipt.Description = $"Receipt cloned from {templateReceipt.ReceiptNumber} at time {DateTime.Now:dd-MMM-yyyy HH:mm:ss}";
        templateReceipt.Id = 0;
        templateReceipt.ReceiptNumber = await GetNextReceiptNoAsync();

        foreach (var detail in templateReceipt.Details)
        {
            detail.Id = 0;
            detail.CreateDate = DateTime.UtcNow;
            detail.Receipt = templateReceipt; // re-assign the navigation property to the new receipt
        }
        await _actionLogService.AddLogAsync(
          $"[Clone] receipt ReceiptNo {templateReceipt.ReceiptNumber} IsStored: {templateReceipt.IsStored} Details {templateReceipt.Details.Count} rows",
          "Inbound", currentUser);

        try
        {
            _dbContext.GetDbSet<InboundReceiptEntity>().Add(templateReceipt);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (true, _localizer["Receipt Cloned successfully"]);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    /// <summary>
    /// Import Excel Beginning Data
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> ImportExcelBeginningData(List<BeginMerchandiseExcel> requests,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        string userName = currentUser.user_name;
        var skus = requests.Select(x => x.SkuCode.Trim()).Distinct().ToList();
        var units = requests.Select(x => x.UnitName.Trim()).Distinct().ToList();
        var wareHouses = requests.Select(x => x.WareHouseName.Trim()).Distinct().ToList();
        var suppliers = requests.Select(x => x.SupplierName.Trim()).Distinct().ToList();

        var locations = requests.Select(x => x.LocationCode?.Trim())
            .Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

        ProccesingAddMatterData(tenantId, units, wareHouses, suppliers, skus, userName);
        await _dbContext.SaveChangesAsync(cancellationToken);

        EnsureStockHasVirutalLocations(tenantId, wareHouses);
        await _dbContext.SaveChangesAsync(cancellationToken);

        int index = 0;
        int insertedCount = 0;
        int totalRequests = requests.Count;

        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var supplierList = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);

        do
        {
            var batchOrders = requests.Skip(index).Take(SystemDefine.BatchSize).ToList();
            var listPutaway = new List<BeginMerchandiseEntity>();
            var items = new List<BeginMerchandiseEntity>();

            foreach (var order in batchOrders)
            {
                var wareHouse = wareHouseList.FirstOrDefault(w => w.WarehouseName == order.WareHouseName)
                    ?? throw new Exception($"Warehouse not found for name: {order.WareHouseName}");
                var supplierId = GetFirstSupplierId(supplierList, order.SupplierName);
                var isPutaway = order.IsPutaway?.Trim().ToLower() == "yes" || order.IsPutaway?.Trim().ToLower() == "true";
                var beginEntity = MapBeginMerchandiseEntity(isPutaway, order, supplierId, wareHouse,
                    tenantId, userName, skuList, uomList, locationList);
                items.Add(beginEntity);

                if (isPutaway)
                {
                    listPutaway.Add(beginEntity);
                }
            }

            try
            {
                // Save Draft
                var res = await SaveBeginMerchandiseAsync(tenantId, items);
                insertedCount += res;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {Index}", index);
                // Optionally, you can choose to break the loop or continue with the next batch
                // break;
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalRequests);

        return insertedCount;
    }

    private BeginMerchandiseEntity MapBeginMerchandiseEntity(bool isPutaway, BeginMerchandiseExcel order,
        int supplierId, WarehouseEntity wareHouse, long tenantId, string userName,
        IQueryable<SkuEntity> skuList, IQueryable<SkuUomEntity> uomList,
        IQueryable<GoodslocationEntity> locationList)
    {
        var sku = skuList.FirstOrDefault(x => x.sku_code == order.SkuCode || x.sku_name == order.SkuCode);
        var uom = uomList.FirstOrDefault(x => x.UnitName == order.UnitName);
        var locationId = GetLocationId(locationList, order.LocationCode, wareHouse, tenantId);

        return new BeginMerchandiseEntity
        {
            TenantId = tenantId,
            IsPutaway = isPutaway,
            WarehouseId = wareHouse.Id,
            SupplierId = supplierId,
            ExpireDate = order.ExpireDate, // You can set this based on the requests if needed
            LocationId = locationId, // You can set this based on the requests if needed
            Quantity = int.TryParse(order.Qty, out var qty) ? qty : 0,
            SkuId = sku?.Id ?? 0, // You can set this based on the requests if needed
            UomId = uom?.Id ?? 0, // You can set this based on the requests if needed
            Creator = userName ?? "System",
            CreatedDate = DateTime.UtcNow
        };
    }

    private async Task<int> SaveBeginMerchandiseAsync(long tenantId, List<BeginMerchandiseEntity> items)
    {
        var queryBegin = _dbContext.GetDbSet<BeginMerchandiseEntity>(tenantId, true);


        foreach (var item in items)
        {
            var existing = await queryBegin.Where(x => x.SkuId == item.SkuId
                && x.WarehouseId == item.WarehouseId && x.SupplierId == item.SupplierId
                && x.ExpireDate == item.ExpireDate && x.LocationId == item.LocationId)
                .AnyAsync();

            if (!existing)
            {
                _dbContext.GetDbSet<BeginMerchandiseEntity>().Add(item);
            }
        }

        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Get Begin Merchandises
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<BeginMerchandiseDto>> GetBeginMerchandiseAsync(CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var queryBegin = _dbContext.GetDbSet<BeginMerchandiseEntity>(tenantId);
        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var supplierList = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);

        var results = from begin in queryBegin
                      join w in wareHouseList on begin.WarehouseId equals w.Id into wh
                      from wareHouse in wh.DefaultIfEmpty()
                      join s in skuList on begin.SkuId equals s.Id into sk
                      from sku in sk.DefaultIfEmpty()
                      join u in uomList on begin.UomId equals u.Id into um
                      from uom in um.DefaultIfEmpty()
                      join sp in supplierList on begin.SupplierId equals sp.Id into su
                      from supplier in su.DefaultIfEmpty()
                      join l in locationList on begin.LocationId equals l.Id into lo
                      from location in lo.DefaultIfEmpty()
                      select new BeginMerchandiseDto
                      {
                          Id = begin.Id,
                          TenantId = begin.TenantId,
                          WarehouseId = begin.WarehouseId,
                          WarehouseName = wareHouse.WarehouseName,
                          SkuId = begin.SkuId,
                          SkuCode = sku.sku_code,
                          SkuName = sku.sku_name,
                          Quantity = begin.Quantity,
                          SupplierId = begin.SupplierId,
                          SupplierName = supplier.supplier_name ?? "",
                          ExpireDate = begin.ExpireDate,
                          UomId = begin.UomId,
                          UnitName = uom.UnitName,
                          LocationId = begin.LocationId,
                          LocationName = location.LocationName,
                          CreatedDate = begin.CreatedDate,
                          Creator = begin.Creator,
                          IsPutaway = begin.IsPutaway
                      };

        return await results.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Delete Begin Merchandise
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool success, string message)> DeleteBeginMerchandise(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var entity = await _dbContext.GetDbSet<BeginMerchandiseEntity>(tenantId, true)
            .Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (entity != null)
        {
            _dbContext.GetDbSet<BeginMerchandiseEntity>().Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (true, _localizer["Deleted successfully"]);
        }
        else
        {
            return (false, _localizer["Entity not found"]);
        }
    }

    /// <summary>
    /// Save Begin Merchandise
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> SaveBeginMerchandise(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId, true);
        var items = await _dbContext.GetDbSet<BeginMerchandiseEntity>(tenantId, true)
            .ToListAsync(cancellationToken: cancellationToken);

        await _actionLogService.AddLogAsync(
           $"[Save Beginning Merchandise] receipt details: {items.Count} rows",
           "Inbound", currentUser);

        foreach (var item in items)
        {
            var stockEntities = await stockList.Where(x => x.sku_id == item.SkuId
                && x.goods_location_id == item.LocationId
                && x.SupplierId == item.SupplierId)
                .ToListAsync(cancellationToken: cancellationToken);

            if (stockEntities.Count != 0)
            {
                var myExpireDate = item.ExpireDate.GetValueOrDefault().Date;

                var stock = stockEntities
                    .FirstOrDefault(x => x.expiry_date.GetValueOrDefault().Date == myExpireDate);

                if (stock != null)
                {
                    stock.actual_qty += item.Quantity.GetValueOrDefault();
                    stock.qty += (int)item.Quantity.GetValueOrDefault();
                    stock.last_update_time = DateTime.UtcNow;
                    _dbContext.GetDbSet<StockEntity>().Update(stock);
                }
                else
                {
                    var palletCode = await _functionHelper.GetFormNoAsync("pallet", "PLT");
                    _dbContext.GetDbSet<StockEntity>().Add(new StockEntity
                    {
                        TenantId = tenantId,
                        sku_id = item.SkuId.GetValueOrDefault(),
                        goods_location_id = item.LocationId.GetValueOrDefault(),
                        expiry_date = item.ExpireDate,
                        qty = (int)item.Quantity.GetValueOrDefault(),
                        actual_qty = item.Quantity.GetValueOrDefault(),
                        SupplierId = item.SupplierId,
                        last_update_time = DateTime.UtcNow,
                        Palletcode = palletCode,
                        PutAwayDate = DateTime.UtcNow
                    });
                }
            }
            else
            {
                var palletCode = await _functionHelper.GetFormNoAsync("pallet", "PLT");
                _dbContext.GetDbSet<StockEntity>().Add(new StockEntity
                {
                    TenantId = tenantId,
                    sku_id = item.SkuId.GetValueOrDefault(),
                    goods_location_id = item.LocationId.GetValueOrDefault(),
                    expiry_date = item.ExpireDate,
                    qty = (int)item.Quantity.GetValueOrDefault(),
                    actual_qty = item.Quantity.GetValueOrDefault(),
                    SupplierId = item.SupplierId,
                    last_update_time = DateTime.UtcNow,
                    Palletcode = palletCode,
                    PutAwayDate = DateTime.UtcNow
                });
            }

            _dbContext.GetDbSet<StockTransactionEntity>().Add(new StockTransactionEntity
            {
                TenantId = tenantId,
                SkuId = item.SkuId.GetValueOrDefault(),
                SupplierId = item.SupplierId,
                Quantity = item.Quantity.GetValueOrDefault(),
                TransactionType = StockTransactionType.Inbound,
                TransactionDate = DateTime.UtcNow,
            });

            _dbContext.GetDbSet<BeginMerchandiseEntity>().Remove(item);
        }

        var rs = await _dbContext.SaveChangesAsync(cancellationToken);

        if (rs > 0)
        {
            return (true, _localizer["Saved successfully"]);
        }
        else
        {
            return (false, _localizer["Entity not found"]);
        }
    }

    /// <summary>
    /// Get Deleted Data with ReceiptStatus = CANCELED
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<InboundReceiptListResponse>> GetDeletedData(CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var hasRole = UserRoleDef.IsAdminRole(currentUser.user_role);
        if (!hasRole) return [];

        var tennantId = currentUser.tenant_id;
        var receipts = _dbContext.GetDbSet<InboundReceiptEntity>(tennantId)
            .Where(x => x.Status == ReceiptStatus.CANCELED);
        var warehouses = _dbContext.GetDbSet<WarehouseEntity>(tennantId)
            .Where(x => x.is_valid);
        var suppliers = _dbContext.GetDbSet<SupplierEntity>(tennantId);
        var details = _dbContext.GetDbSet<InboundReceiptDetailEntity>();

        var baseQuery = from r in receipts
                        join d in details on r.Id equals d.ReceiptId into wDetail
                        from d in wDetail.DefaultIfEmpty()
                        join w in warehouses on r.WarehouseId equals w.Id into wGroup
                        from w in wGroup.DefaultIfEmpty()
                        join s in suppliers on r.SupplierId equals s.Id into sGroup
                        from s in sGroup.DefaultIfEmpty()
                        select new InboundReceiptListResponse
                        {
                            Id = r.Id,
                            IsStored = r.IsStored.GetValueOrDefault(),
                            ReceiptNo = r.ReceiptNumber ?? string.Empty,
                            ReceiptType = r.Type ?? string.Empty,
                            SupplierId = r.SupplierId,
                            SupplierName = s.supplier_name ?? string.Empty,
                            WarehouseId = r.WarehouseId,
                            WarehouseName = w != null ? w.WarehouseName : string.Empty,
                            Status = (int)r.Status,
                            CreateDate = r.CreateDate,
                            TotalQty = r.Details.Sum(d => (int?)d.Quantity) ?? 0,
                            Description = r.Description,
                            LastUpdatedDate = r.LastUpdateTime
                        };

        return await baseQuery.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create Bulk Receipts Async
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(int id, string message)> CreateBulkReceiptsAsync(CreateBulkReceiptRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        await _actionLogService.AddLogAsync(
            $"[Bulk-Receipts] Creating new receipt with number {request.ReceiptNo}, " +
            $"Type: {request.Type}, SupplierId: {request.SupplierId}, " +
            $"WarehouseId: {request.WarehouseId}, IsDraft: {request.IsDraft}",
            "Inbound", currentUser);

        return await CreateInternalAsync(request, currentUser, cancellationToken, true, request.StoredData);
    }

    private async Task<(int id, string message)> CreateInternalAsync(
            CreateReceiptRequest request,
            CurrentUser currentUser,
            CancellationToken cancellationToken,
            bool multiPallets = false,
            IEnumerable<AvailablePallet>? storedData = null)
    {
        var receiptDbSet = _dbContext.GetDbSet<InboundReceiptEntity>();
        var availablePallets = storedData ?? [];
        if (multiPallets && request.IsStored)
        {
            availablePallets = await EnsureStoredPallets(request, storedData, currentUser, cancellationToken);
        }

        string errorMsg = await CheckValidateInputAsync(request, receiptDbSet, availablePallets, currentUser, cancellationToken);

        if (!string.IsNullOrEmpty(errorMsg)) return (0, errorMsg);

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (!request.IsStored)
        {
            var virtualLocationId = await GetOrCreateVirtualLocationAsync(
                request.WarehouseId, currentUser, cancellationToken);

            foreach (var details in request.Details)
            {
                details.LocationId = virtualLocationId;
                details.PalletCode = null;
            }
        }

        // bussiness rule 
        // handle the mapping for new sku and supplier
        await EnsureSkuSupplierMappingAsync(
                 request.SupplierId,
                 [.. request.Details.Select(d => d.SkuId).Distinct()],
                 cancellationToken);

        // Create Header Entity
        var inboundReceiptEntity = new InboundReceiptEntity
        {
            ReceiptNumber = request.ReceiptNo.Trim(),
            WarehouseId = request.WarehouseId,
            SupplierId = request.SupplierId,
            Type = request.Type,
            Creator = currentUser.user_name,
            CreateDate = request.CreatedDate ?? DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow,
            Description = request.Description,
            Status = request.IsDraft ? ReceiptStatus.DRAFT : ReceiptStatus.NEW,
            TenantId = currentUser.tenant_id,
            IsStored = request.IsStored,
            Deliverer = request.Deliverer,
            InvoiceNumber = request.InvoiceNumber,
            MultiPallets = multiPallets,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate
        };

        // Details
        foreach (var item in request.Details)
        {
            inboundReceiptEntity.Details.Add(new InboundReceiptDetailEntity
            {
                SkuId = item.SkuId,
                Quantity = item.Quantity,
                SkuUomId = item.SkuUomId,
                CreateDate = DateTime.UtcNow,
                ExpiryDate = item.ExpiryDate,
                LocationId = item.LocationId,
                PalletCode = item.PalletCode,
                SourceNumber = item.SourceNumber,
                ReqQty = item.Quantity
            });
        }

        foreach (var pallet in availablePallets)
        {
            inboundReceiptEntity.PalletDetails.Add(new InboundPalletEntity
            {
                PalletCode = pallet.PalletName,
                LocationId = pallet.LocationId,
                SkuId = pallet.SkuId,
                SkuUomId = pallet.SkuUomId ?? 0,
                Quantity = pallet.Balance,
                CreateDate = DateTime.UtcNow,
                TenantId = currentUser.tenant_id,
                PlanningDate = request.ExpectedDeliveryDate.GetValueOrDefault(DateTime.UtcNow),
            });
        }

        await receiptDbSet.AddAsync(inboundReceiptEntity, cancellationToken);

        // save Draft
        if (request.IsDraft)
        {
            if (request.IsStored)
            {
                await UpdatePalletStatusAsync(request.Details, currentUser.tenant_id, cancellationToken, availablePallets);
            }
        }
        else
        {
            // store the value
            if (request.IsStored)
            {
                await CreateInboundIntegrationCommandsAsync(inboundReceiptEntity, request,
                    currentUser, cancellationToken, availablePallets);

                await UpdatePalletStatusAsync(request.Details, currentUser.tenant_id, cancellationToken, availablePallets);
            }
            // not store , directly update stock, and no wcs task needed
            else
            {
                inboundReceiptEntity.Status = ReceiptStatus.COMPLETE;
                await UpdateStockAsync(request.Details, request.ReceiptNo,
                    request.SupplierId, currentUser, cancellationToken, availablePallets);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        //if(AvailablePallet)
        return (inboundReceiptEntity.Id, _localizer["save_success"]);
    }

    private async Task<IEnumerable<AvailablePallet>> EnsureStoredPallets(CreateReceiptRequest request,
        IEnumerable<AvailablePallet>? storedData, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var skuIds = new List<int>();
        var results = new List<AvailablePallet>();
        foreach (var detail in request.Details)
        {
            var items = (storedData ?? []).Where(x => x.SkuId == detail.SkuId);
            var dataBalance = items.Sum(x => x.Balance);
            if (dataBalance >= detail.Quantity)
            {
                //TODO _palletService
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.PalletName)) continue;

                    var pallet = await _palletService.GenaratePalletCodeAsync(currentUser, cancellationToken);
                    item.PalletName = pallet?.PalletCode ?? "";
                }

                skuIds.Add(detail.SkuId);
                results.AddRange(items);
                continue;
            }

            var palletRequest = new CalculatorPalletRequest
            {
                WarehouseId = request.WarehouseId,
                Details =
                [
                    new InboundDetailItem
                    {
                        SkuId = detail.SkuId,
                        Quantity = (int) detail.Quantity
                    }
                ]
            };

            var data = await _warehouseService.GetCalculatorPalletsAsync(palletRequest, currentUser, cancellationToken);
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.PalletName)) continue;

                var pallet = await _palletService.GenaratePalletCodeAsync(currentUser, cancellationToken);
                item.PalletName = pallet?.PalletCode ?? "";
            }

            results.AddRange(data);
        }

        return results;
    }

    private async Task<string> CheckValidateInputAsync(CreateReceiptRequest request,
        DbSet<InboundReceiptEntity> receiptDbSet,
        IEnumerable<AvailablePallet> availablePallets,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.Details == null || request.Details.Count == 0)
        {
            return _localizer["Details are required"];
        }

        if (request.WarehouseId <= 0)
        {
            return _localizer["Warehouse is required"];
        }

        if ((availablePallets == null || !availablePallets.Any()) && request.SupplierId <= 0)
        {
            return _localizer["Supplier is required"];
        }

        if (request.ReceiptNo is null)
        {
            return _localizer["Receipt number is required"];
        }

        var receiptNo = request.ReceiptNo.Trim();
        var isDuplicateReceiptNo = await receiptDbSet.AsNoTracking()
            .AnyAsync(r => r.ReceiptNumber == receiptNo, cancellationToken);

        if (isDuplicateReceiptNo)
        {
            return _localizer["Receipt number already exists"];
        }

        var today = DateTime.UtcNow.Date;
        var invalidExpiry = request.Details
            .FirstOrDefault(d => d.ExpiryDate.HasValue && d.ExpiryDate.GetValueOrDefault().Date < today);

        if (invalidExpiry != null)
        {
            return _localizer["Expiry date cannot be in the past"];
        }

        if (request.IsStored)
        {
            if (availablePallets != null && availablePallets.Any())
            {
                var locIds = availablePallets.Select(x => x.LocationId).Distinct();
                var locations = await _dbContext
                    .GetDbSet<GoodslocationEntity>(currentUser.tenant_id)
                    .Where(x => x.IsValid && x.GoodsLocationType == GoodsLocationTypeEnum.StorageSlot)
                    .Where(x => locIds.Contains(x.Id))
                    .CountAsync(cancellationToken);

                if (locations != locIds.Count())
                    return _localizer["There aren’t recognized as a Storage place"];
            }
            else
            {
                var invalidDetail = request.Details
                     .FirstOrDefault(d => d.LocationId is null or <= 0);
                if (invalidDetail != null)
                {
                    return _localizer["Location is required when storing"];
                }

                var requestConflict = request.Details
                                       .Where(d => !string.IsNullOrWhiteSpace(d.PalletCode))
                                       .Select(d => new
                                       {
                                           PalletCode = d.PalletCode!.Trim(),
                                           LocationId = d.LocationId!.Value
                                       })
                                       .GroupBy(x => x.PalletCode)
                                       .FirstOrDefault(g => g.Select(x => x.LocationId).Distinct().Count() > 1);

                if (requestConflict != null)
                {
                    return _localizer["One pallet cannot belong to multiple locations in the same receipt"];
                }
            }
        }
        else
        {
            var virtualLocationId = await GetOrCreateVirtualLocationAsync(
                request.WarehouseId, currentUser, cancellationToken);

            if (virtualLocationId <= 0)
            {
                _logger.LogError("Failed to get or create virtual location for WarehouseId={WarehouseId}, TenantId={TenantId}",
                    request.WarehouseId, currentUser.tenant_id);
                return _localizer["Failed to resolve virtual location"];
            }
        }

        foreach (var item in request.Details)
        {
            if (item.SkuId <= 0 || item.Quantity <= 0 || item.SkuUomId <= 0)
            {
                return _localizer[$"Invalid detail item {item.SkuId}"];
            }
        }

        return "";
    }

    /// <summary>
    /// Share Inbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetShareInbound(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var query = _dbContext.GetDbSet<InboundReceiptEntity>(currentUser.tenant_id);

        var receipt = await query.FirstOrDefaultAsync(x => x.Id == id && x.Status == ReceiptStatus.COMPLETE, cancellationToken);
        if (receipt == null) return "";

        var queryShared = _dbContext.GetDbSet<SharedReceiptEntity>(currentUser.tenant_id)
            .Where(x => x.InboundReceipt);

        var sharedItem = await queryShared.FirstOrDefaultAsync(x => x.ReceiptId == id, cancellationToken);

        if (sharedItem != null) return sharedItem.ShareKey;

        string shareKey = $"{GenarationHelper.GetRandomPassword(13)}{Random.Shared.Next(1, 100)}";
        await _actionLogService.AddLogAsync(
            $"[Sharing] Receipt with number {receipt.ReceiptNumber}, Type: {receipt.Type}, " +
            $"SupplierId: {receipt.SupplierId}, WarehouseId: {receipt.WarehouseId} shareKey={shareKey}",
            "Inbound", currentUser);

        _dbContext.GetDbSet<SharedReceiptEntity>().Add(new SharedReceiptEntity
        {
            Creator = currentUser.user_name,
            InboundReceipt = true,
            ReceiptId = id,
            ReceiptNumber = receipt.ReceiptNumber,
            ShareKey = shareKey,
            CreateDate = DateTime.UtcNow,
            TenantId = currentUser.tenant_id
        });

        int rows = await _dbContext.SaveChangesAsync(cancellationToken);

        return rows > 0 ? shareKey : "";
    }

    /// <summary>
    /// Get Receipt Sharing Url
    /// </summary>
    /// <param name="sharingUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<InboundReceiptDetailedDto> GetReceiptSharingUrl(string sharingUrl, CancellationToken cancellationToken)
    {
        var queryShared = _dbContext.GetDbSet<SharedReceiptEntity>()
            .AsNoTracking()
            .Where(x => x.InboundReceipt && x.ShareKey == sharingUrl);

        var sharedItem = await queryShared.FirstOrDefaultAsync(cancellationToken);
        if (sharedItem != null && sharedItem.ReceiptId.GetValueOrDefault() > 0)
        {
            return await GetDataSharing(sharedItem.ReceiptId.GetValueOrDefault(), cancellationToken);
        }

        return new InboundReceiptDetailedDto();
    }

    private async Task<InboundReceiptDetailedDto> GetDataSharing(int receiptId, CancellationToken cancellationToken)
    {
        var receiptEntity = await _dbContext.GetDbSet<InboundReceiptEntity>()
                                           .Include(r => r.Details)
                                           .FirstOrDefaultAsync(x => x.Id == receiptId && x.Status != ReceiptStatus.CANCELED, cancellationToken);

        if (receiptEntity is null)
        {
            return new InboundReceiptDetailedDto();
        }

        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>()
                                    .FirstOrDefaultAsync(w => w.Id == receiptEntity.WarehouseId, cancellationToken);
        var supplier = await _dbContext.GetDbSet<SupplierEntity>()
                                    .FirstOrDefaultAsync(s => s.Id == receiptEntity.SupplierId, cancellationToken);

        var detailList = receiptEntity.Details ?? [];
        var skuIds = detailList.Select(d => d.SkuId).Distinct().ToList();
        var uomIds = detailList.Select(d => d.SkuUomId).Distinct().ToList();
        var skuLookup = await _dbContext.GetDbSet<SkuEntity>().AsNoTracking()
                                        .Where(s => skuIds.Contains(s.Id))
                                        .ToDictionaryAsync(s => s.Id, s => new
                                        {
                                            Code = s.sku_code,
                                            Name = s.sku_name,
                                        }, cancellationToken);

        var uomLookup = await _dbContext.GetDbSet<SkuUomEntity>().AsNoTracking()
                                      .Where(u => uomIds.Contains(u.Id))
                                      .ToDictionaryAsync(u => u.Id, u => u.UnitName, cancellationToken);

        var locationIds = detailList
                         .Where(d => d != null && d.LocationId.HasValue)
                         .Select(d => d.LocationId!.Value)
                         .Distinct()
                         .ToList();

        var locationLookup = await _dbContext.GetDbSet<GoodslocationEntity>()
                                             .Where(l => locationIds.Contains(l.Id))
                                             .ToDictionaryAsync(
                                                 l => l.Id,
                                                 l => l.LocationName ?? string.Empty,
                                                 cancellationToken
                                             );

        return new InboundReceiptDetailedDto
        {
            Id = receiptEntity.Id,
            ReceiptNo = receiptEntity.ReceiptNumber,
            WarehouseId = receiptEntity.WarehouseId,
            WarehouseName = warehouse?.WarehouseName ?? string.Empty,
            SupplierId = receiptEntity.SupplierId,
            SupplierName = supplier?.supplier_name ?? string.Empty,
            Type = receiptEntity.Type ?? string.Empty,
            IsStored = receiptEntity.IsStored.GetValueOrDefault(),
            CreatedDate = receiptEntity.CreateDate,
            Status = (int)receiptEntity.Status,
            Description = receiptEntity.Description,
            Deliverer = receiptEntity.Deliverer,
            InvoiceNumber = receiptEntity.InvoiceNumber,
            Address = warehouse?.address ?? string.Empty,
            MultiPallets = receiptEntity.MultiPallets ?? false,
            ExpectedDeliveryDate = receiptEntity.ExpectedDeliveryDate,
            Priority = receiptEntity.Priority,
            Details = [.. detailList.Select(d =>
            {
                skuLookup.TryGetValue(d.SkuId, out var skuInfo);
                bool isException = !d.LocationId.HasValue && string.IsNullOrEmpty(d.PalletCode);

                return new InboundReceiptDetailItemDto
                {
                    Id = d.Id,
                    SkuId = d.SkuId,
                    SkuCode = skuInfo?.Code ?? string.Empty,
                    SkuName = skuInfo?.Name ?? string.Empty,
                    Quantity = d.Quantity,
                    SkuUomId = d.SkuUomId,
                    UnitName = uomLookup.GetValueOrDefault(d.SkuUomId) ?? string.Empty,
                    LocationId = d.LocationId,
                    LocationName = (d.LocationId.HasValue && locationLookup.TryGetValue(d.LocationId.Value, out var locName)) ? locName : string.Empty,
                    PalletCode = d.PalletCode,
                    ExpiryDate = d.ExpiryDate,
                    IsException = isException,
                    SourceNumber = d.SourceNumber,
                };
            })]
        };
    }
}
