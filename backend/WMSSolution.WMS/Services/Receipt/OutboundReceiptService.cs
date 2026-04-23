using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Core.MultiTenancy;
using WMSSolution.Core.Utility;
using WMSSolution.Shared;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.RBAC;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.OutboundGateway;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;
using WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;
using WMSSolution.WMS.Entities.ViewModels.StockTransaction;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.Receipt;

namespace WMSSolution.WMS.Services.Receipt;

/// <summary>
/// Outbound receipt service implementation
/// </summary>
public class OutboundReceiptService(SqlDBContext dbContext,
                                    IStringLocalizer<MultiLanguage> localizer,
                                    ILogger<OutboundReceiptService> logger,
                                    IActionLogService actionLogService,
                                    FunctionHelper functionHelper) : IOutboundReceiptService
{
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly IStringLocalizer<MultiLanguage> _localizer = localizer;
    private readonly ILogger<OutboundReceiptService> _logger = logger;
    private readonly FunctionHelper _functionHelper = functionHelper;
    private readonly IActionLogService _actionLogService = actionLogService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(int id, string message)> CreateAsync(CreateOutboundReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var targetStatus = request.IsDraft ? ReceiptStatus.DRAFT : ReceiptStatus.NEW;
        var isCreateTask = !request.IsDraft;

        await _actionLogService.AddLogAsync(
            $"Creating new receipt with number {request.ReceiptNo}, Type: {request.Type}, CustomerId: {request.CustomerId}, WarehouseId: {request.WarehouseId}, IsDraft: {request.IsDraft}",
            "Outbound", currentUser);

        return await CreateInternalAsync(request, currentUser, cancellationToken, targetStatus, isCreateTask);
    }

    #region CreateAsync helper bussiness methods

    /// <summary>
    /// Create outbound receipt with different status (draft vs submit)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="targetStatus"></param>
    /// <param name="isCreateTask"></param>
    /// <returns></returns>
    private async Task<(int id, string message)> CreateInternalAsync(
                                 CreateOutboundReceiptRequest request,
                                 CurrentUser currentUser,
                                 CancellationToken cancellationToken,
                                 ReceiptStatus targetStatus,
                                 bool isCreateTask)
    {
        var tenantId = currentUser.tenant_id;
        var outboundReceiptDbSet = _dbContext.GetDbSet<OutBoundReceiptEntity>();
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        (bool IsValid, string? ErrorMessage) = await ValidateUpdateRequestAsync(request, tenantId, cancellationToken);
        if (!IsValid)
        {
            _logger.LogWarning("Validation failed for outbound receipt creation. Error: {ErrorMessage}", ErrorMessage);
            return (0, ErrorMessage);
        }

        var entity = new OutBoundReceiptEntity
        {
            ReceiptNumber = request.ReceiptNo.Trim(),
            WarehouseId = request.WarehouseId,
            CustomerId = request.CustomerId,
            Type = request.Type,
            OutboundGatewayId = request.OutboundGatewayId,
            Creator = currentUser.user_name,
            CreateDate = DateTime.UtcNow,
            ReceiptDate = request.ReceiptDate ?? DateTime.UtcNow,
            Status = targetStatus,
            Description = request.Description,
            StartPickingTime = request.StartPickingTime,
            ExpectedShipDate = request.ExpectedShipDate,
            TenantId = tenantId,
            Consignee = request.Consignee ?? "",
            Details = [.. request.Details.Select(item => new OutBoundReceiptDetailEntity
            {
                SkuId = item.SkuId,
                Quantity = item.Quantity,
                LocationId = item.LocationId,
                PalletCode = item.PalletCode,
                SkuUomId = item.SkuUomId,
                ReqQty = item.Quantity
            })]
        };

        await outboundReceiptDbSet.AddAsync(entity, cancellationToken);

        if (isCreateTask)
        {
            var stockCheckResult = await VerifyStockAvailabilityAsync(request.Details, tenantId, cancellationToken);
            if (!stockCheckResult.IsValid)
            {
                _logger.LogWarning("Stock verification failed for outbound receipt creation. Error: {ErrorMessage}", stockCheckResult.ErrorMessage);
                return (0, stockCheckResult.ErrorMessage);
            }

            var virtualLocationIds = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                .Where(x => x.WarehouseId == request.WarehouseId && x.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var virtualDetails = request.Details
                .Where(d => d.LocationId.HasValue && virtualLocationIds.Contains(d.LocationId.Value))
                .ToList();

            var physicalDetails = request.Details
                .Where(d => !d.LocationId.HasValue || !virtualLocationIds.Contains(d.LocationId.Value))
                .ToList();

            if (virtualDetails.Count != 0)
            {
                _logger.LogInformation("Deducting stock for {Count} virtual location details", virtualDetails.Count);
                await DeductStockAsync(virtualDetails, request.ReceiptNo, request.CustomerId, tenantId, cancellationToken);
            }

            if (physicalDetails.Count != 0)
            {
                request.Details = physicalDetails;
                await CreateOutboundIntegrationCommandsAsync(entity, request, currentUser, cancellationToken);
            }

            if (isCreateTask && physicalDetails.Count == 0 && virtualDetails.Count != 0)
            {
                _logger.LogInformation("No physical details to create outbound tasks. Marking receipt as COMPLETE.");
                entity.Status = ReceiptStatus.COMPLETE;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (entity.Id, _localizer["save_success"]);
    }

    /// <summary>
    /// Validate outbound receipt request
    /// </summary>
    private async Task<(bool IsValid, string ErrorMessage)> ValidateUpdateRequestAsync(
        CreateOutboundReceiptRequest request,
        long tenantId,
        CancellationToken cancellationToken)
    {

        if (request.Details is null || request.Details.Count == 0)
        {
            return (false, _localizer["Details are required"]);
        }

        if (request.WarehouseId <= 0)
        {
            return (false, _localizer["Warehouse is required"]);
        }

        if (request.CustomerId <= 0)
        {
            return (false, _localizer["Customer is required"]);
        }

        if (request.OutboundGatewayId <= 0)
        {
            return (false, _localizer["Outbound gateway is required"]);
        }

        if (string.IsNullOrWhiteSpace(request.ReceiptNo))
        {
            return (false, _localizer["Receipt number is required"]);
        }

        foreach (var item in request.Details)
        {
            //|| item.SkuUomId <= 0
            if (item.SkuId <= 0 || item.Quantity <= 0)
            {
                return (false, _localizer["Invalid detail item"]);
            }

            if (item.LocationId is null or <= 0)
            {
                return (false, _localizer["Location is required for outbound"]);
            }
        }

        var receiptNo = request.ReceiptNo.Trim();
        var isDuplicate = await _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .AnyAsync(r => r.ReceiptNumber == receiptNo, cancellationToken);

        if (isDuplicate)
        {
            return (false, _localizer["Receipt number already exists"]);
        }

        return (true, string.Empty);
    }

    private async Task CreateOutboundIntegrationCommandsAsync(
                            OutBoundReceiptEntity receipt,
                            CreateOutboundReceiptRequest request,
                            CurrentUser currentUser,
                            CancellationToken cancellationToken)
    {
        var ouboundTaskDbSet = _dbContext.GetDbSet<OutboundEntity>();
        var historyDbSet = _dbContext.GetDbSet<IntegrationHistory>();
        var mappingDbSet = _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>();
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

        if (groupedKeys.Count == 0)
        {
            return;
        }

        var palletCodes = groupedKeys.Select(x => x.PalletCode).Distinct().ToList();
        var locationIds = groupedKeys.Select(x => x.LocationId).Distinct().ToList();

        var existingActive = await ouboundTaskDbSet.Where(x => x.TenantId == receipt.TenantId
                                        && x.IsActive
                                        && palletCodes.Contains(x.PalletCode)
                                        && locationIds.Contains(x.LocationId)
                                        && x.Status == IntegrationStatus.Ready)
                                        .ToListAsync(cancellationToken);

        var existingSet = existingActive.Select(x => $"{x.PalletCode}|{x.LocationId}").ToHashSet();

        var sharedTaskCode = GenarationHelper.GenerateTaskCode();
        var newOutboundTasks = groupedKeys
                                .Where(x => !existingSet.Contains($"{x.PalletCode}|{x.LocationId}"))
                                .Select(x => new OutboundEntity
                                {
                                    TaskCode = sharedTaskCode,
                                    PalletCode = x.PalletCode,
                                    LocationId = x.LocationId,
                                    GatewayId = receipt.OutboundGatewayId,
                                    Priority = request.Priority,
                                    TenantId = currentUser.tenant_id,
                                    Status = IntegrationStatus.Ready,
                                    CreatedDate = now,
                                    IsActive = true
                                })
                                .ToList();

        if (newOutboundTasks.Count > 0)
        {
            await _dbContext.GetDbSet<OutboundEntity>().AddRangeAsync(newOutboundTasks, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var histories = newOutboundTasks.Select(task => new IntegrationHistory
            {
                PalletCode = task.PalletCode,
                TaskCode = task.TaskCode,
                HistoryType = HistoryType.Outbound,
                LocationId = task.LocationId,
                Description = $"Outbound Retrieval Task Created from Receipt {receipt.ReceiptNumber}",
                TenantId = currentUser.tenant_id,
                Status = task.Status,
                CreatedDate = task.CreatedDate,
                Priority = task.Priority,
                IsActive = task.IsActive,
            }).ToList();

            await historyDbSet.AddRangeAsync(histories, cancellationToken);
        }

        var allRelevantTasks = await ouboundTaskDbSet.Where(x => x.TenantId == currentUser.tenant_id
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

        var mappingsToInsert = new List<ReceiptDetailOutboundIntegrationEntity>();

        if (receipt.Details != null && receipt.Details.Count != 0)
        {
            foreach (var detail in receipt.Details)
            {
                if (string.IsNullOrWhiteSpace(detail.PalletCode) || detail.LocationId is not > 0)
                {
                    continue;
                }

                var key = new
                {
                    PalletCode = detail.PalletCode.Trim(),
                    LocationId = detail.LocationId.Value
                };

                if (taskLookup.TryGetValue(key, out var taskId))
                {
                    mappingsToInsert.Add(new ReceiptDetailOutboundIntegrationEntity
                    {
                        ReceiptDetailId = detail.Id,
                        OutboundId = (int)taskId,
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

    /// <summary>
    /// Verify that stock is available for all detail lines.
    /// Converts to base UOM before comparing.
    /// </summary>
    private async Task<(bool IsValid, string ErrorMessage)> VerifyStockAvailabilityAsync(
        List<CreateOutboundReceiptDetailDto> details,
        long tenantId,
        CancellationToken cancellationToken)
    {
        var stockEntities = _dbContext.GetDbSet<StockEntity>(tenantId);

        // Pre-load UOM data for conversion
        var uomIds = details.Select(d => d.SkuUomId).Distinct().ToList();
        var uoms = await _dbContext.GetDbSet<SkuUomEntity>()
            .Where(u => uomIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        foreach (var item in details)
        {
            var baseQty = ConvertToBaseQty(item.Quantity, item.SkuUomId, uoms);
            var palletCode = item.PalletCode?.Trim() ?? string.Empty;

            var stock = await stockEntities
                .FirstOrDefaultAsync(s =>
                    s.sku_id == item.SkuId
                    && s.goods_location_id == item.LocationId
                    && (s.Palletcode ?? "") == palletCode, cancellationToken);

            if (stock is null)
            {
                return (false, string.Format(
                    "Insufficient stock for SKU {0} at location {1}",
                    item.SkuId, item.LocationId));
            }
        }

        return (true, string.Empty);
    }

    private static decimal ConvertToBaseQty(
       decimal quantity, int uomId, Dictionary<int, SkuUomEntity> uoms)
    {
        return quantity;
        //TODO later: if UOM conversion is needed,
        //implement this method. For now, we assume all quantities are in base UOM.

        //if (!uoms.TryGetValue(uomId, out var uom) || uom.IsBaseUnit)
        //{
        //    return quantity;
        //}

        //return uom.Operator == ConversionOperator.Multiply
        //    ? quantity * uom.ConversionRate
        //    : quantity / uom.ConversionRate;
    }

    private async Task DeductStockAsync(
         List<CreateOutboundReceiptDetailDto> details,
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

            transactionRequests.Add(new AddStockTransactionRequest(
                StockId: stock.Id,
                QuantityChange: -item.Quantity,
                SkuId: item.SkuId,
                SkuUomId: item.SkuUomId,
                TransactionType: StockTransactionType.Outbound,
                TenantId: tenantId,
                CustomerId: customerId,
                CustomerName: customerName,
                ConversionRate: currentConversionRate,
                UnitName: currentUnitName,
                RefReceipt: receiptNumber
            ));
        }

        await LogStockTransactionAsync(transactionRequests, cancellationToken);
    }

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
            SupplierName = r.SupplierName,
            CustomerId = r.CustomerId,
            CustomerName = r.CustomerName
        }).ToList();

        var transactionDbSet = _dbContext.GetDbSet<StockTransactionEntity>();
        await transactionDbSet.AddRangeAsync(transaction, cancellationToken);
    }

    #endregion CreateAsync helper methods
    /// <summary>
    /// Get next outbound receipt number
    /// </summary>
    public async Task<string> GetNextReceiptNoAsync()
    {
        //var prefix = _dbContext.GetDbSet<Global>
        return await _functionHelper.GetFormNoAsync("outbound_receipt", "PXK-");
    }

    /// <summary>
    /// Outbound receipt pagination query
    /// </summary>
    /// <param name="pageSearch">Pagination and filter criteria</param>
    /// <param name="currentUser">The current user performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of outbound receipts</returns>
    public async Task<(List<OutboundReceiptListResponse> data, int totals)> PageOutboundReceiptAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var filterObjects = pageSearch.searchObjects.Where(x => x.Sort == 0).ToList();
        var sortObject = pageSearch.searchObjects.FirstOrDefault(x => x.Sort != 0);

        QueryCollection queries = [];
        if (filterObjects.Count != 0)
        {
            filterObjects.ForEach(queries.Add);
        }

        var receipt = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.Status != ReceiptStatus.CANCELED);
        var warehouse = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var outboundGateway = _dbContext.GetDbSet<OutboundGatewayEntity>(tenantId);
        var customer = _dbContext.GetDbSet<CustomerEntity>(tenantId);

        var query = from r in receipt
                    join w in warehouse on r.WarehouseId equals w.Id into wj
                    from w in wj.DefaultIfEmpty()
                    join c in customer on r.CustomerId equals c.Id into cj
                    from c in cj.DefaultIfEmpty()
                    join g in outboundGateway on r.OutboundGatewayId equals g.Id into gj
                    from g in gj.DefaultIfEmpty()
                    select new OutboundReceiptListResponse
                    {
                        Id = r.Id,
                        ReceiptNo = r.ReceiptNumber,
                        Type = r.Type,
                        CustomerId = r.CustomerId,
                        CustomerName = c != null ? c.customer_name : string.Empty,
                        WarehouseId = r.WarehouseId,
                        WarehouseName = w != null ? w.WarehouseName : string.Empty,
                        WarehouseAddress = w != null ? w.address : string.Empty,
                        CreatedDate = r.CreateDate,
                        Creator = r.Creator ?? "",
                        CreateDate = r.CreateDate,
                        ReceiptDate = r.ReceiptDate,
                        Consignee = r.Consignee ?? "",
                        ExpectedShipDate = r.ExpectedShipDate,
                        StartPickingTime = r.StartPickingTime,
                        Status = (int)r.Status,
                        OutboundGatewayId = r.OutboundGatewayId,
                        OutBoundGatewayName = g != null ? g.GatewayName : string.Empty,
                        TotalQty = r.Details.Sum(d => (int?)d.Quantity) ?? 0,
                        Description = r.Description
                    };

        var asGroupedExpression = queries.AsGroupedExpression<OutboundReceiptListResponse>();
        if (asGroupedExpression != null)
        {
            query = query.Where(asGroupedExpression);
        }

        var totals = await query.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return ([], 0);
        }

        IOrderedQueryable<OutboundReceiptListResponse> sortedQuery;

        if (sortObject != null && !string.IsNullOrEmpty(sortObject.Name))
        {
            bool isDesc = sortObject.Sort == 2; // 1 as ASC, 2 as DESC
            sortedQuery = sortObject.Name.ToLower() switch
            {
                "receiptnumber" => isDesc ? query.OrderByDescending(x => x.ReceiptNo) : query.OrderBy(x => x.ReceiptNo),
                "type" => isDesc ? query.OrderByDescending(x => x.Type) : query.OrderBy(x => x.Type),
                "customername" => isDesc ? query.OrderByDescending(x => x.CustomerName) : query.OrderBy(x => x.CustomerName),
                "warehousename" => isDesc ? query.OrderByDescending(x => x.WarehouseName) : query.OrderBy(x => x.WarehouseName),
                "outboundgatewayname" => isDesc ? query.OrderByDescending(x => x.OutBoundGatewayName) : query.OrderBy(x => x.OutBoundGatewayName),
                "createdate" => isDesc ? query.OrderByDescending(x => x.CreateDate) : query.OrderBy(x => x.CreateDate),
                "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderByDescending(x => x.CreateDate)
            };
        }
        else
        {
            sortedQuery = query
               .OrderBy(t =>
                       t.Status == 1 ? 1 :
                       t.Status == 2 ? 2 :
                       t.Status == 0 ? 3 :
                       t.Status == 3 ? 4 : 5)
                .ThenByDescending(x => x.CreateDate);
        }

        var list = await sortedQuery.Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                                    .Take(pageSearch.pageSize)
                                    .ToListAsync(cancellationToken);

        return (list, totals);

    }

    /// <summary>
    /// Cancel outbound 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> CancelAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var receipt = await _dbContext.GetDbSet<OutBoundReceiptEntity>(currentUser.tenant_id, isTracking: true)
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

            var taskIds = await _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>()
            .Where(m => detailIds.Contains(m.ReceiptDetailId))
            .Select(m => m.OutboundId)
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

        await _actionLogService.AddLogAsync(
            $"Cancelled receipt Id {receipt.Id} with number {receipt.ReceiptNumber}, Type: {receipt.Type}, CustomerId: {receipt.CustomerId}, WarehouseId: {receipt.WarehouseId}",
            "Outbound", currentUser);

        return (true, _localizer["Receipt cancelled successfully"]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OutboundReceiptDetailedDto?> GetReceiptByIdAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var receipt = await _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
                                       .Include(r => r.Details)
                                       .FirstOrDefaultAsync(r => r.Id == id && r.Status != ReceiptStatus.CANCELED, cancellationToken);

        if (receipt == null)
        {
            return null;
        }

        var skuIds = receipt.Details.Select(d => d.SkuId).Distinct().ToList();
        var locations = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                                    .Where(l => receipt.Details.Select(d => d.LocationId).Contains(l.Id))
                                    .ToDictionaryAsync(l => l.Id, cancellationToken);
        var locationIds = receipt.Details.Where(d => d.LocationId.HasValue).Select(d => d.LocationId!.Value).Distinct().ToList();
        var uomIds = receipt.Details.Select(d => d.SkuUomId).Distinct().ToList();


        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
                                            .Where(w => w.Id == receipt.WarehouseId)
                                            .FirstOrDefaultAsync(cancellationToken);

        //var warehouseName = await _dbContext.GetDbSet<WarehouseEntity>(tenantId)
        //                                    .Where(w => w.Id == receipt.WarehouseId)
        //                                    .Select(w => w.WarehouseName)
        //                                    .FirstOrDefaultAsync(cancellationToken);

        var customerName = await _dbContext.GetDbSet<CustomerEntity>(tenantId)
                                            .Where(c => c.Id == receipt.CustomerId)
                                            .Select(c => c.customer_name)
                                            .FirstOrDefaultAsync(cancellationToken);

        var gatewayName = await _dbContext.GetDbSet<OutboundGatewayEntity>(tenantId)
                                            .Where(g => g.Id == receipt.OutboundGatewayId)
                                            .Select(g => g.GatewayName)
                                            .FirstOrDefaultAsync(cancellationToken);

        var skuDictionary = await _dbContext.GetDbSet<SkuEntity>()
                                            .Where(s => skuIds.Contains(s.Id))
                                            .Select(s => new { s.Id, s.sku_code, s.sku_name })
                                            .ToDictionaryAsync(s => s.Id, cancellationToken);
        var locationDict = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                                           .Where(l => locationIds.Contains(l.Id))
                                           .ToDictionaryAsync(l => l.Id, l => l.LocationName, cancellationToken);

        var uomDict = await _dbContext.GetDbSet<SkuUomEntity>(tenantId)
                                      .Where(u => uomIds.Contains(u.Id))
                                      .ToDictionaryAsync(u => u.Id, u => u.UnitName, cancellationToken);

        var sharingUrl = await _dbContext.GetDbSet<SharedReceiptEntity>(tenantId)
            .Where(x => !x.InboundReceipt && x.ReceiptId == id)
            .Select(x => x.ShareKey)
            .FirstOrDefaultAsync(cancellationToken);

        var receiptDto = new OutboundReceiptDetailedDto
        {
            Id = receipt.Id,
            ReceiptNo = receipt.ReceiptNumber,
            WarehouseId = receipt.WarehouseId,
            WarehouseName = warehouse?.WarehouseName ?? string.Empty,
            ReceiptDate = receipt.ReceiptDate,
            WarehouseAddress = warehouse?.address ?? string.Empty,
            Type = receipt.Type ?? string.Empty,
            CreatedDate = receipt.CreateDate,
            CustomerId = receipt.CustomerId,
            CustomerName = customerName ?? string.Empty,
            OutboundGatewayId = receipt.OutboundGatewayId,
            OutboundGatewayName = gatewayName ?? string.Empty,
            Status = (int)receipt.Status,
            SharingUrl = sharingUrl ?? "",
            Priority = receipt.Priority,
            StartPickingTime = receipt.StartPickingTime,
            ExpectedShipDate = receipt.ExpectedShipDate,
            Consignee = receipt.Consignee ?? string.Empty,
            Description = receipt.Description ?? string.Empty,
            Details = [.. receipt.Details.Select(d =>
            {
                skuDictionary.TryGetValue(d.SkuId, out var skuInfo);
                locationDict.TryGetValue(d.LocationId ?? 0, out var locName);
                uomDict.TryGetValue(d.SkuUomId, out var uomName);

                return new OutboundReceiptDetailItemDto
                {
                    Id = d.Id,
                    SkuId = d.SkuId,
                    SkuCode = skuInfo?.sku_code ?? string.Empty,
                    SkuName = skuInfo?.sku_name ?? string.Empty,
                    Quantity = d.Quantity,
                    SkuUomId = d.SkuUomId,
                    UnitName = uomName ?? string.Empty,
                    LocationId = d.LocationId,
                    LocationName = locName,
                    PalletCode = d.PalletCode,
                    IsException = (d.PalletCode is null && d.LocationId is null)
                };
            })]
        };

        return receiptDto;
    }


    /// <summary>
    /// update outbound receipt
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool IsSuccess, string ErrorMessage)> UpdateAsync(int id, UpdateOutboundReceiptRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingReceipt = await _dbContext.GetDbSet<OutBoundReceiptEntity>()
                                           .Include(r => r.Details)
                                           .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, cancellationToken);

        if (existingReceipt is null) return (false, _localizer["Receipt not found"]);
        if (existingReceipt.Status != ReceiptStatus.DRAFT)
        {
            return (false, _localizer["Only draft receipts can be updated"]);
        }

        var (IsValid, ErrorMessage) = await ValidateUpdateRequestAsync(id, request, tenantId, cancellationToken);
        if (!IsValid)
        {
            return (false, ErrorMessage);
        }

        existingReceipt.WarehouseId = request.WarehouseId;
        existingReceipt.CustomerId = request.CustomerId;
        existingReceipt.Type = request.Type;
        existingReceipt.OutboundGatewayId = request.OutboundGatewayId;
        existingReceipt.LastUpdateTime = DateTime.UtcNow;
        existingReceipt.Description = request.Description;
        existingReceipt.Consignee = request.Consignee;
        existingReceipt.ExpectedShipDate = request.ExpectedShipDate;
        existingReceipt.StartPickingTime = request.StartPickingTime;

        var incomingDetailIds = request.Details.Where(d => d.Id > 0).Select(d => d.Id).ToList();
        var detailsToRemove = existingReceipt.Details.Where(d => !incomingDetailIds.Contains(d.Id)).ToList();

        if (detailsToRemove.Count != 0)
        {
            _dbContext.RemoveRange(detailsToRemove);
            foreach (var removed in detailsToRemove)
            {
                existingReceipt.Details.Remove(removed);
            }
        }

        foreach (var reqItem in request.Details)
        {
            if (reqItem.Id > 0)
            {
                var existingDetail = existingReceipt.Details.FirstOrDefault(d => d.Id == reqItem.Id);
                if (existingDetail != null)
                {
                    existingDetail.SkuId = reqItem.SkuId;
                    existingDetail.Quantity = reqItem.Quantity;
                    existingDetail.LocationId = reqItem.LocationId;
                    existingDetail.PalletCode = reqItem.PalletCode;
                    existingDetail.SkuUomId = reqItem.SkuUomId;
                    existingDetail.DispatchId = reqItem.DispatchId;
                }
            }
            else
            {
                existingReceipt.Details.Add(new OutBoundReceiptDetailEntity
                {
                    SkuId = reqItem.SkuId,
                    Quantity = reqItem.Quantity,
                    LocationId = reqItem.LocationId,
                    PalletCode = reqItem.PalletCode,
                    SkuUomId = reqItem.SkuUomId,
                    DispatchId = reqItem.DispatchId
                });
            }
        }

        var activeDetails = request.Details.ToList();

        if (request.IsUpgradeStatus)
        {
            var stockCheckResult = await VerifyStockAvailabilityAsync(activeDetails, tenantId, cancellationToken);
            if (!stockCheckResult.IsValid) return (false, stockCheckResult.ErrorMessage);

            var virtualLocationIds = await _dbContext.GetDbSet<GoodslocationEntity>(tenantId)
                .Where(x => x.WarehouseId == request.WarehouseId && x.GoodsLocationType == GoodsLocationTypeEnum.VirtualLocation)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var virtualDetails = activeDetails.Where(d => d.LocationId.HasValue && virtualLocationIds.Contains(d.LocationId.Value)).ToList();
            var physicalDetails = activeDetails.Where(d => !d.LocationId.HasValue || !virtualLocationIds.Contains(d.LocationId.Value)).ToList();

            if (virtualDetails.Count != 0)
            {
                existingReceipt.Status = ReceiptStatus.COMPLETE;
                await DeductStockAsync(virtualDetails, request.ReceiptNo, request.CustomerId, tenantId, cancellationToken);
            }

            if (physicalDetails.Count != 0)
            {
                existingReceipt.Status = ReceiptStatus.NEW;
                request.Details = physicalDetails;
                await CreateOutboundIntegrationCommandsAsync(existingReceipt, request, currentUser, cancellationToken);
            }
        }


        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _actionLogService.AddLogAsync(
            $"Updated receipt Id {existingReceipt.Id} with number {existingReceipt.ReceiptNumber}, Type: {existingReceipt.Type}, CustomerId: {existingReceipt.CustomerId}, WarehouseId: {existingReceipt.WarehouseId}",
            "Outbound", currentUser);

        return (true, string.Empty);
    }

    private async Task CreateOutboundIntegrationCommandsAsync(
                          OutBoundReceiptEntity receipt,
                          UpdateOutboundReceiptRequest request,
                          CurrentUser currentUser,
                          CancellationToken cancellationToken)
    {
        var ouboundTaskDbSet = _dbContext.GetDbSet<OutboundEntity>();
        var historyDbSet = _dbContext.GetDbSet<IntegrationHistory>();
        var mappingDbSet = _dbContext.GetDbSet<ReceiptDetailOutboundIntegrationEntity>();
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

        if (groupedKeys.Count == 0)
        {
            return;
        }

        var palletCodes = groupedKeys.Select(x => x.PalletCode).Distinct().ToList();
        var locationIds = groupedKeys.Select(x => x.LocationId).Distinct().ToList();

        var existingActive = await ouboundTaskDbSet.Where(x => x.TenantId == receipt.TenantId
                                        && x.IsActive
                                        && palletCodes.Contains(x.PalletCode)
                                        && locationIds.Contains(x.LocationId)
                                        && x.Status == IntegrationStatus.Ready)
                                        .ToListAsync(cancellationToken);

        var existingSet = existingActive.Select(x => $"{x.PalletCode}|{x.LocationId}").ToHashSet();

        var sharedTaskCode = GenarationHelper.GenerateTaskCode();
        var newOutboundTasks = groupedKeys
                                .Where(x => !existingSet.Contains($"{x.PalletCode}|{x.LocationId}"))
                                .Select(x => new OutboundEntity
                                {
                                    TaskCode = sharedTaskCode,
                                    PalletCode = x.PalletCode,
                                    LocationId = x.LocationId,
                                    GatewayId = receipt.OutboundGatewayId,
                                    Priority = request.Priority,
                                    TenantId = currentUser.tenant_id,
                                    Status = IntegrationStatus.Ready,
                                    CreatedDate = now,
                                    IsActive = true
                                })
                                .ToList();

        if (newOutboundTasks.Count > 0)
        {
            await _dbContext.GetDbSet<OutboundEntity>().AddRangeAsync(newOutboundTasks, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var histories = newOutboundTasks.Select(task => new IntegrationHistory
            {
                PalletCode = task.PalletCode,
                TaskCode = task.TaskCode,
                HistoryType = HistoryType.Outbound,
                LocationId = task.LocationId,
                Description = $"Outbound Retrieval Task Created from Receipt {receipt.ReceiptNumber}",
                TenantId = currentUser.tenant_id,
                Status = task.Status,
                CreatedDate = task.CreatedDate,
                Priority = task.Priority,
                IsActive = task.IsActive,
            }).ToList();

            await historyDbSet.AddRangeAsync(histories, cancellationToken);
        }

        var allRelevantTasks = await ouboundTaskDbSet.Where(x => x.TenantId == currentUser.tenant_id
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
                LocationId = x.LocationId
            })
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Id).First().Id
            );

        var mappingsToInsert = new List<ReceiptDetailOutboundIntegrationEntity>();

        if (receipt.Details != null && receipt.Details.Count != 0)
        {
            foreach (var detail in receipt.Details)
            {
                if (string.IsNullOrWhiteSpace(detail.PalletCode) || detail.LocationId is not > 0)
                {
                    continue;
                }

                var key = new
                {
                    PalletCode = detail.PalletCode.Trim(),
                    LocationId = detail.LocationId.Value
                };

                if (taskLookup.TryGetValue(key, out var taskId))
                {
                    mappingsToInsert.Add(new ReceiptDetailOutboundIntegrationEntity
                    {
                        ReceiptDetailId = detail.Id,
                        OutboundId = (int)taskId,
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


    private async Task<(bool IsValid, string ErrorMessage)> ValidateUpdateRequestAsync(
                                                            int currentId,
                                                            UpdateOutboundReceiptRequest request,
                                                            long tenantId,
                                                            CancellationToken cancellationToken)
    {
        if (request.Details is null || request.Details.Count == 0)
            return (false, _localizer["Details are required"]);

        if (request.WarehouseId <= 0) return (false, _localizer["Warehouse is required"]);
        if (request.CustomerId <= 0) return (false, _localizer["Customer is required"]);
        if (request.OutboundGatewayId <= 0) return (false, _localizer["Outbound gateway is required"]);
        if (string.IsNullOrWhiteSpace(request.ReceiptNo)) return (false, _localizer["Receipt number is required"]);

        foreach (var item in request.Details)
        {
            if (item.SkuId <= 0 || item.Quantity <= 0 || item.SkuUomId <= 0)
                return (false, _localizer["Invalid detail item"]);

            if (item.LocationId is null or <= 0)
                return (false, _localizer["Location is required for outbound"]);
        }

        var receiptNo = request.ReceiptNo.Trim();

        var isDuplicate = await _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .AnyAsync(r => r.ReceiptNumber == receiptNo && r.Id != currentId, cancellationToken);

        if (isDuplicate)
            return (false, _localizer["Receipt number already exists"]);

        return (true, string.Empty);
    }
    private async Task<(bool IsValid, string ErrorMessage)> VerifyStockAvailabilityAsync(
                                                                List<UpdateOutboundReceiptDetailDto> details,
                                                                long tenantId,
                                                                CancellationToken cancellationToken)
    {
        var stockEntities = _dbContext.GetDbSet<StockEntity>(tenantId);
        var uomIds = details.Select(d => d.SkuUomId).Distinct().ToList();
        var uoms = await _dbContext.GetDbSet<SkuUomEntity>()
            .Where(u => uomIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        foreach (var item in details)
        {
            var baseQty = ConvertToBaseQty(item.Quantity, item.SkuUomId, uoms);
            var palletCode = item.PalletCode?.Trim() ?? string.Empty;

            var stock = await stockEntities
                .FirstOrDefaultAsync(s =>
                    s.sku_id == item.SkuId
                    && s.goods_location_id == item.LocationId
                    && (s.Palletcode ?? "") == palletCode, cancellationToken);

            if (stock is null)
            {
                return (false, string.Format(
                    "Insufficient stock for SKU {0} at location {1}",
                    item.SkuId, item.LocationId));
            }
        }

        return (true, string.Empty);
    }

    private async Task DeductStockAsync(
        List<UpdateOutboundReceiptDetailDto> details,
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

            transactionRequests.Add(new AddStockTransactionRequest(
                 StockId: stock.Id,
                 QuantityChange: -item.Quantity,
                 SkuId: item.SkuId,
                 SkuUomId: item.SkuUomId,
                 TransactionType: StockTransactionType.Outbound,
                 TenantId: tenantId,
                 CustomerId: customerId,
                 CustomerName: customerName,
                 ConversionRate: currentConversionRate,
                 UnitName: currentUnitName,
                 RefReceipt: receiptNumber
             ));
        }
        await LogStockTransactionAsync(transactionRequests, cancellationToken);
    }

    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<int> ImportExcelData(List<OutboundOrderExcel> requests,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var customers = requests.Select(x => x.CustomerName.Trim()).Distinct().ToList();
        var tenantId = currentUser.tenant_id;
        string userName = currentUser.user_name;

        int index = 0;
        int insertedCount = 0;
        int totalRequests = requests.Count;
        var uniqueOrders = requests.Select(x => new
        {
            Code = x.OrderCode.Trim(),
            WareHouseName = x.WareHouseName.Trim(),
            IsLocation = !string.IsNullOrEmpty(x.LocationCode?.Trim()),
            GatewayName = x.GatewayName.Trim(),
            CustomerName = x.CustomerName.Trim()
        }).GroupBy(x => new { x.Code, x.WareHouseName, x.CustomerName, x.GatewayName, x.IsLocation })
        .Select(g => g.First());

        EnsureGatewayNameHasValue(requests, tenantId);
        ProccesingAddMatterData(tenantId, customers, userName);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var queryReceipt = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId, true);
        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var customerList = _dbContext.GetDbSet<CustomerEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        var gatewayList = _dbContext.GetDbSet<OutboundGatewayEntity>(tenantId);
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);

        do
        {
            var batchOrders = uniqueOrders.Skip(index).Take(SystemDefine.BatchSize).ToList();
            var listLocation = new List<OutBoundReceiptEntity>();
            var items = new List<OutBoundReceiptEntity>();

            foreach (var order in batchOrders)
            {
                var wareHouse = wareHouseList.FirstOrDefault(w => w.WarehouseName == order.WareHouseName)
                    ?? throw new Exception($"Warehouse not found for name: {order.WareHouseName}");
                var customerId = GetFirstCustomerId(customerList, order.CustomerName);
                var gateway = await gatewayList.FirstOrDefaultAsync(g =>
                    g.WarehouseId == wareHouse.Id && g.GatewayName == order.GatewayName);

                var outboundReceiptEntity = MapOutboundReceiptEntity(stockList, gateway?.Id ?? 0,
                    customerId, wareHouse, order.Code, requests, tenantId, userName,
                    wareHouseList, skuList, uomList, locationList);
                items.Add(outboundReceiptEntity);
                if (order.IsLocation)
                {
                    listLocation.Add(outboundReceiptEntity);
                }
            }

            try
            {
                // save Draft
                var res = await SaveNewOrdersAsync(items, queryReceipt);
                insertedCount += res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {Index}", index);
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalRequests);

        return insertedCount;
    }

    private async Task<int> SaveNewOrdersAsync(List<OutBoundReceiptEntity> items,
        IQueryable<OutBoundReceiptEntity> queryReceipt)
    {
        foreach (var item in items)
        {
            if (!queryReceipt.Any(r => r.ReceiptNumber == item.ReceiptNumber))
            {
                _dbContext.GetDbSet<OutBoundReceiptEntity>().Add(item);
            }
        }

        return await _dbContext.SaveChangesAsync();
    }

    private void EnsureGatewayNameHasValue(List<OutboundOrderExcel> requests, long tenantId)
    {
        var uniqueGateways = requests.Select(x => new
        {
            WareHouseName = x.WareHouseName.Trim(),
            GatewayName = x.GatewayName.Trim()
        })
            .GroupBy(x => new { x.WareHouseName, x.GatewayName })
            .Select(g => g.First());

        var wareHouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var gatewayList = _dbContext.GetDbSet<OutboundGatewayEntity>(tenantId);

        foreach (var gateway in uniqueGateways)
        {
            var wareHouse = wareHouseList.FirstOrDefault(w => w.WarehouseName == gateway.WareHouseName)
                ?? throw new Exception($"Warehouse not found for name: {gateway.WareHouseName}");

            if (!gatewayList.Any(g => g.WarehouseId == wareHouse.Id && g.GatewayName == gateway.GatewayName))
            {
                _dbContext.GetDbSet<OutboundGatewayEntity>().Add(new OutboundGatewayEntity
                {
                    WarehouseId = wareHouse.Id,
                    GatewayName = gateway.GatewayName,
                    TenantId = tenantId,
                    CreateTime = DateTime.UtcNow,
                    LastUpdateTime = DateTime.UtcNow
                });
            }
        }
    }

    private OutBoundReceiptEntity MapOutboundReceiptEntity(IQueryable<StockEntity> stockList,
        int gatewayId, int customerId,
        WarehouseEntity wareHouse, string code, List<OutboundOrderExcel> requests,
        long tenantId, string userName, IQueryable<WarehouseEntity> wareHouseList,
        IQueryable<SkuEntity> skuList, IQueryable<SkuUomEntity> uomList,
        IQueryable<GoodslocationEntity> locationList)
    {
        return new OutBoundReceiptEntity
        {
            ReceiptNumber = code,
            WarehouseId = wareHouse.Id,
            CustomerId = customerId,
            Type = "SALES",
            OutboundGatewayId = gatewayId, // Assuming gateway is not provided in the Excel, set to null or handle accordingly
            Status = ReceiptStatus.DRAFT,
            CreateDate = DateTime.UtcNow,
            Creator = userName,
            LastUpdateTime = DateTime.UtcNow,
            TenantId = tenantId,
            Details = GetDetails(stockList, code, requests, skuList, uomList, locationList)
        };
    }
    
    private ICollection<OutBoundReceiptDetailEntity> GetDetails(IQueryable<StockEntity> stockList,
        string code, List<OutboundOrderExcel> requests,
        IQueryable<SkuEntity> skuList, IQueryable<SkuUomEntity> uomList,
        IQueryable<GoodslocationEntity> locationList)
    {
        var items = requests.Where(r => r.OrderCode.Trim() == code);
        var rs = new List<OutBoundReceiptDetailEntity>();
        foreach (var item in items)
        {
            var sku = skuList.FirstOrDefault(s => s.sku_code == item.SkuCode.Trim() ||
                s.sku_name == item.SkuCode.Trim())
                 ?? throw new Exception($"SKU not found for code: {item.SkuCode}");

            var uom = uomList.FirstOrDefault(u => u.UnitName == item.UnitName);
            var locationId = locationList.FirstOrDefault(l => l.LocationName == item.LocationCode.Trim())?.Id;
            var palletCode = stockList.FirstOrDefault(s => s.sku_id == sku.Id && s.goods_location_id == locationId)?.Palletcode;

            rs.Add(new OutBoundReceiptDetailEntity
            {
                SkuId = sku.Id,
                Quantity = decimal.TryParse(item.Qty, out var qty) ? qty : 0,
                SkuUomId = uom != null ? uom.Id : 0,
                LocationId = locationId,
                PalletCode = palletCode?.Trim()
            });
        }

        return rs;
    }

    private int GetFirstCustomerId(IQueryable<CustomerEntity> customerList, string customerName)
    {
        return customerList.FirstOrDefault(c => c.customer_name == customerName)?.Id
            ?? throw new Exception($"Customer not found for name: {customerName}");
    }

    private void ProccesingAddMatterData(long tenantId, List<string> customers, string userName)
    {
        var customerList = _dbContext.GetDbSet<CustomerEntity>(tenantId);
        foreach (var customerName in customers)
        {
            if (!customerList.Any(c => c.customer_name == customerName))
            {
                _dbContext.GetDbSet<CustomerEntity>().Add(new CustomerEntity
                {
                    customer_name = customerName,
                    TenantId = tenantId,
                    create_time = DateTime.UtcNow,
                    creator = userName,
                    last_update_time = DateTime.UtcNow,
                    customer_code = $"CUS-{GenarationHelper.GetRandomPassword(6)}",
                    is_valid = true
                });
            }
        }
    }

    /// <summary>
    /// Revert Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<(bool success, string message)> RevertOutboundAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var receipt = await _dbContext.GetDbSet<OutBoundReceiptEntity>(currentUser.tenant_id, true)
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
    /// Clone Outbound Async
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, string message)> CloneOutboundAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var templateReceipt = await _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
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

        try
        {
            _dbContext.GetDbSet<OutBoundReceiptEntity>().Add(templateReceipt);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (true, _localizer["Receipt Cloned successfully"]);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    /// <summary>
    /// Get Deleted Data = ReceiptStatus is CANCELED
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<OutboundReceiptListResponse>> GetDeletedData(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var hasRole = UserRoleDef.IsAdminRole(currentUser.user_role);
        if (!hasRole) return [];

        var tenantId = currentUser.tenant_id;
        var receipt = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.CANCELED);

        var warehouse = _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .Where(x => x.is_valid);
        var outboundGateway = _dbContext.GetDbSet<OutboundGatewayEntity>(tenantId);
        var customer = _dbContext.GetDbSet<CustomerEntity>(tenantId);

        var query = from r in receipt
                    join w in warehouse on r.WarehouseId equals w.Id into wj
                    from w in wj.DefaultIfEmpty()
                    join c in customer on r.CustomerId equals c.Id into cj
                    from c in cj.DefaultIfEmpty()
                    join g in outboundGateway on r.OutboundGatewayId equals g.Id into gj
                    from g in gj.DefaultIfEmpty()
                    select new OutboundReceiptListResponse
                    {
                        Id = r.Id,
                        ReceiptNo = r.ReceiptNumber,
                        Type = r.Type,
                        CustomerId = r.CustomerId,
                        CustomerName = c != null ? c.customer_name : string.Empty,
                        WarehouseId = r.WarehouseId,
                        WarehouseName = w != null ? w.WarehouseName : string.Empty,
                        WarehouseAddress = w != null ? w.address : string.Empty,
                        CreatedDate = r.CreateDate,
                        Creator = r.Creator ?? "",
                        CreateDate = r.CreateDate,
                        ReceiptDate = r.ReceiptDate,
                        Consignee = r.Consignee ?? "",
                        ExpectedShipDate = r.ExpectedShipDate,
                        StartPickingTime = r.StartPickingTime,
                        Status = (int)r.Status,
                        OutboundGatewayId = r.OutboundGatewayId,
                        OutBoundGatewayName = g != null ? g.GatewayName : string.Empty,
                        TotalQty = r.Details.Sum(d => (int?)d.Quantity) ?? 0,
                        Description = r.Description,
                        LastUpdatedDate = r.LastUpdateTime
                    };

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Share Outbound
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetShareOutbound(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var query = _dbContext.GetDbSet<OutBoundReceiptEntity>(currentUser.tenant_id);

        var receipt = await query.FirstOrDefaultAsync(x => x.Id == id && x.Status == ReceiptStatus.COMPLETE, cancellationToken);
        if (receipt == null) return "";

        var queryShared = _dbContext.GetDbSet<SharedReceiptEntity>(currentUser.tenant_id)
            .Where(x => x.InboundReceipt);

        var sharedItem = await queryShared.FirstOrDefaultAsync(x => x.ReceiptId == id, cancellationToken);

        if (sharedItem != null) return sharedItem.ShareKey;

        string shareKey = $"{Random.Shared.Next(1, 100)}{GenarationHelper.GetRandomPassword(13)}";
        await _actionLogService.AddLogAsync(
            $"[Sharing] Receipt with number {receipt.ReceiptNumber}, Type: {receipt.Type}, " +
            $"CustomerId: {receipt.CustomerId}, WarehouseId: {receipt.WarehouseId} shareKey={shareKey}",
            "Outbound", currentUser);

        _dbContext.GetDbSet<SharedReceiptEntity>().Add(new SharedReceiptEntity
        {
            Creator = currentUser.user_name,
            InboundReceipt = false,
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
    public async Task<OutboundReceiptDetailedDto> GetReceiptSharingUrl(string sharingUrl, CancellationToken cancellationToken)
    {
        var queryShared = _dbContext.GetDbSet<SharedReceiptEntity>()
            .AsNoTracking()
            .Where(x => !x.InboundReceipt && x.ShareKey == sharingUrl);

        var sharedItem = await queryShared.FirstOrDefaultAsync(cancellationToken);
        if (sharedItem != null && sharedItem.ReceiptId.GetValueOrDefault() > 0)
        {
            return await GetDataSharing(sharedItem.ReceiptId.GetValueOrDefault(), cancellationToken);
        }

        return new OutboundReceiptDetailedDto();
    }

    private async Task<OutboundReceiptDetailedDto> GetDataSharing(int receiptId, CancellationToken cancellationToken)
    {
        var receipt = await _dbContext.GetDbSet<OutBoundReceiptEntity>()
            .AsNoTracking()
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.Status != ReceiptStatus.CANCELED, cancellationToken);

        if (receipt == null)
        {
            return new OutboundReceiptDetailedDto();
        }

        var skuIds = receipt.Details.Select(d => d.SkuId).Distinct().ToList();
        var locations = await _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking()
                                    .Where(l => receipt.Details.Select(d => d.LocationId).Contains(l.Id))
                                    .ToDictionaryAsync(l => l.Id, cancellationToken);
        var locationIds = receipt.Details.Where(d => d.LocationId.HasValue).Select(d => d.LocationId!.Value).Distinct().ToList();
        var uomIds = receipt.Details.Select(d => d.SkuUomId).Distinct().ToList();


        var warehouse = await _dbContext.GetDbSet<WarehouseEntity>().AsNoTracking()
                                            .Where(w => w.Id == receipt.WarehouseId)
                                            .FirstOrDefaultAsync(cancellationToken);

        var customerName = await _dbContext.GetDbSet<CustomerEntity>().AsNoTracking()
                                            .Where(c => c.Id == receipt.CustomerId)
                                            .Select(c => c.customer_name)
                                            .FirstOrDefaultAsync(cancellationToken);

        var gatewayName = await _dbContext.GetDbSet<OutboundGatewayEntity>().AsNoTracking()
                                            .Where(g => g.Id == receipt.OutboundGatewayId)
                                            .Select(g => g.GatewayName)
                                            .FirstOrDefaultAsync(cancellationToken);

        var skuDictionary = await _dbContext.GetDbSet<SkuEntity>().AsNoTracking()
                                            .Where(s => skuIds.Contains(s.Id))
                                            .Select(s => new { s.Id, s.sku_code, s.sku_name })
                                            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var locationDict = await _dbContext.GetDbSet<GoodslocationEntity>().AsNoTracking()
                                           .Where(l => locationIds.Contains(l.Id))
                                           .ToDictionaryAsync(l => l.Id, l => l.LocationName, cancellationToken);

        var uomDict = await _dbContext.GetDbSet<SkuUomEntity>().AsNoTracking()
                                      .Where(u => uomIds.Contains(u.Id))
                                      .ToDictionaryAsync(u => u.Id, u => u.UnitName, cancellationToken);

        var receiptDto = new OutboundReceiptDetailedDto
        {
            Id = receipt.Id,
            ReceiptNo = receipt.ReceiptNumber,
            WarehouseId = receipt.WarehouseId,
            WarehouseName = warehouse?.WarehouseName ?? string.Empty,
            ReceiptDate = receipt.ReceiptDate,
            WarehouseAddress = warehouse?.address ?? string.Empty,
            Type = receipt.Type ?? string.Empty,
            CreatedDate = receipt.CreateDate,
            CustomerId = receipt.CustomerId,
            CustomerName = customerName ?? string.Empty,
            OutboundGatewayId = receipt.OutboundGatewayId,
            OutboundGatewayName = gatewayName ?? string.Empty,
            Status = (int)receipt.Status,
            Priority = receipt.Priority,
            StartPickingTime = receipt.StartPickingTime,
            ExpectedShipDate = receipt.ExpectedShipDate,
            Consignee = receipt.Consignee ?? string.Empty,
            Description = receipt.Description ?? string.Empty,
            Details = [.. receipt.Details.Select(d =>
            {
                skuDictionary.TryGetValue(d.SkuId, out var skuInfo);
                locationDict.TryGetValue(d.LocationId ?? 0, out var locName);
                uomDict.TryGetValue(d.SkuUomId, out var uomName);

                return new OutboundReceiptDetailItemDto
                {
                    Id = d.Id,
                    SkuId = d.SkuId,
                    SkuCode = skuInfo?.sku_code ?? string.Empty,
                    SkuName = skuInfo?.sku_name ?? string.Empty,
                    Quantity = d.Quantity,
                    SkuUomId = d.SkuUomId,
                    UnitName = uomName ?? string.Empty,
                    LocationId = d.LocationId,
                    LocationName = locName,
                    PalletCode = d.PalletCode,
                    IsException = (d.PalletCode is null && d.LocationId is null)
                };
            })]
        };

        return receiptDto;
    }
}
