using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.MasterData;
using WMSSolution.Shared.RBAC;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.Models.Stock;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Reports;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.Reports;

namespace WMSSolution.WMS.Services.Reports;

/// <summary>
/// Reports Service
/// </summary>
/// <param name="dbContext"></param>
/// <param name="actionLogService"></param>
/// <param name="logger"></param>
public class ReportService(SqlDBContext dbContext,
    IActionLogService actionLogService,
    ILogger<ReportService> logger) : IReportService
{
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly ILogger<ReportService> _logger = logger;
    private readonly IActionLogService _actionLogService = actionLogService;

    /// <summary>
    /// Get Inventories
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<WarehouseInventoryReport>> GetInventories(InventoryReportRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var query = _dbContext.GetDbSet<StockTransactionEntity>(tenantId);
        var queryOpening = _dbContext.GetDbSet<StockTransactionEntity>(tenantId);
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);

        if (request.FromDate.HasValue)
        {
            queryOpening = queryOpening.Where(x => x.TransactionDate.Date < request.FromDate.Value.Date);
            query = query.Where(x => x.TransactionDate.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate.Date <= request.ToDate.Value.Date);
        }

        var skuIds = (request.SkuIds ?? []).Where(id => id > 0);
        if (skuIds.Any())
        {
            query = query.Where(x => skuIds.Contains(x.SkuId));
        }

        var stockIds = query.Select(x => x.StockId).Distinct().ToList();
        var locationIds = await stockList
                            .Where(s => stockIds.Contains(s.Id))
                            .Select(s => s.goods_location_id)
                            .Distinct()
                            .ToListAsync(cancellationToken);

        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        if (locationIds.Count != 0)
        {
            locationList = locationList.Where(x => locationIds.Contains(x.Id));
        }

        if (request.WarehouseId.GetValueOrDefault(0) > 0)
        {
            locationList = locationList.Where(x => x.WarehouseId == request.WarehouseId.GetValueOrDefault());
        }

        var results = new List<WarehouseInventoryReport>();

        var warehouseIds = locationList.Select(x => x.WarehouseId).Distinct().ToList();
        foreach (var warehouseId in warehouseIds)
        {
            var warehouse = await warehouseList
                .FirstOrDefaultAsync(x => x.Id == warehouseId, cancellationToken);

            if (warehouse != null)
            {
                var locIds = await locationList
                    .Where(x => x.WarehouseId == warehouseId)
                    .Select(x => x.Id).ToListAsync(cancellationToken);


                var reportItems = await (
                    from tran in query
                    join stock in stockList on tran.StockId equals stock.Id
                    where locIds.Contains(stock.goods_location_id)

                    join link in _dbContext.GetDbSet<SkuUomLinkEntity>()
                    on tran.SkuId equals link.SkuId into linkGroup
                    from link in linkGroup
                        .Where(l => l.IsBaseUnit)
                        .DefaultIfEmpty()

                    join uom in _dbContext.GetDbSet<SkuUomEntity>(tenantId)
                        on link.SkuUomId equals uom.Id into uomGroup
                    from uom in uomGroup.DefaultIfEmpty()

                    select new InventoryReportItem
                    {
                        SkuId = tran.SkuId,
                        UnitId = link != null ? link.SkuUomId : tran.SkuUomId,
                        Unit = uom != null ? uom.UnitName : (tran.UnitName ?? ""),
                        Quantity = tran.Quantity,
                        LocationId = stock.goods_location_id
                    })
                    .ToListAsync(cancellationToken);


                var details = new List<InventoryReportItem>();
                var sIds = reportItems.Select(x => x.SkuId).Distinct();
                foreach (var skuId in sIds)
                {
                    var sku = skuList.FirstOrDefault(x => x.Id == skuId);
                    var items = reportItems.Where(x => x.SkuId == skuId);
                    var reportItem = items.FirstOrDefault();

                    var inwardQuantity = items.Where(d => d.Quantity > 0).Sum(d => d.Quantity);
                    var outwardQuantity = items.Where(d => d.Quantity < 0).Sum(d => d.Quantity);

                    var openingBalance = await (
                                    from tran in queryOpening
                                    join stock in stockList on tran.StockId equals stock.Id
                                    where tran.SkuId == skuId && locIds.Contains(stock.goods_location_id)
                                    select tran.Quantity)
                                    .SumAsync(cancellationToken);

                    details.Add(new InventoryReportItem
                    {
                        WarehouseName = warehouse.WarehouseName,
                        ItemCode = sku?.sku_code ?? "",
                        ItemName = sku?.sku_name ?? "",
                        InwardQuantity = inwardQuantity,
                        OutwardQuantity = outwardQuantity * (-1),
                        Unit = reportItem?.Unit ?? "",
                        OpeningBalance = openingBalance,
                        //InwardCost = reportItem.InwardCost,
                        //OpeningCost = reportItem.OpeningCost,
                        //OutwardCost = reportItem.OutwardCost,
                        SkuId = skuId,
                        UnitId = reportItem?.UnitId,
                    });
                }

                results.Add(new WarehouseInventoryReport
                {
                    WarehouseAddress = warehouse.address,
                    WarehouseId = warehouseId,
                    WarehouseName = warehouse.WarehouseName,
                    Items = details
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Get Inventory Cards
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<InventoryCardItem>> GetInventoryCards(InventoryReportRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var inboundList = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);
        var outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);

        var queryOpening = GetQueryInventoryOpenning(tenantId, request);
        var dataInbound = GetQueryInventoryInbound(tenantId, request);
        var dataOutbound = GetQueryInventoryOutbound(tenantId, request);
        var queryClosed = GetQueryInventoryClosed(tenantId, request);

        var results = new List<InventoryCardItem>();
        foreach (var skuId in request.SkuIds)
        {
            var data = await dataInbound
                .Where(x => x.SkuId == skuId)
                .ToListAsync(cancellationToken);

            var dt = await dataOutbound
                .Where(x => x.SkuId == skuId)
                .ToListAsync(cancellationToken);

            data.AddRange(dt);

            var closedBalance = await queryOpening.Where(x => x.SkuId == skuId)
                .SumAsync(x => x.Quantity, cancellationToken);

            if (data.Count > 0 || closedBalance > 0)
            {
                results.Add(new InventoryCardItem
                {
                    ReceiptDate = request.FromDate,
                    Description = "Tồn đầu kỳ",
                    ClosedBalance = closedBalance
                });
            }

            if (data.Count == 0)
            {
                var closingQty = await queryClosed.Where(x => x.SkuId == skuId)
                    .SumAsync(x => x.Quantity, cancellationToken);

                results.Add(new InventoryCardItem
                {
                    ReceiptDate = request.ToDate,
                    Description = "Tồn cuối kỳ",
                    ClosedBalance = closingQty
                });

                continue;
            }

            foreach (var item in data.OrderBy(x => x.TransactionDate))
            {
                var cardItem = new InventoryCardItem
                {
                    InReceiptId = item.InReceiptId,
                    SkuId = item.SkuId,
                    OutReceiptId = item.OutReceiptId,
                    InwardQuantity = item.InwardQuantity,
                    OutwardQuantity = item.OutwardQuantity,
                    TransactionDate = item.TransactionDate
                };

                if (item.InReceiptId.GetValueOrDefault(0) > 0)
                {
                    var inbound = await inboundList
                        .FirstOrDefaultAsync(x => x.Id == item.InReceiptId.GetValueOrDefault(0), cancellationToken);
                    if (inbound != null)
                    {
                        cardItem.GoodsReceiptNote = inbound.ReceiptNumber;
                        cardItem.Description = inbound.Description ?? "";
                        cardItem.ReceiptDate = inbound.CreateDate;
                    }
                }
                else
                {
                    var outbound = await outboundList
                        .FirstOrDefaultAsync(x => x.Id == item.OutReceiptId.GetValueOrDefault(0), cancellationToken);
                    if (outbound != null)
                    {
                        cardItem.GoodsIssueNote = outbound.ReceiptNumber;
                        cardItem.Description = outbound.Description ?? "";
                        cardItem.ReceiptDate = outbound.ReceiptDate.GetValueOrDefault(outbound.CreateDate);
                    }
                }

                closedBalance += (item.InwardQuantity - item.OutwardQuantity);
                cardItem.ClosedBalance = closedBalance;
                results.Add(cardItem);
            }

            results.Add(new InventoryCardItem
            {
                ReceiptDate = request.ToDate,
                Description = "Tồn cuối kỳ",
                ClosedBalance = closedBalance
            });
        }

        return results ?? [];
    }

    /// <summary>
    /// Inventory In-Out Statements
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<InOutStatementDto>> GetInventoryInOutStatements(InventoryReportRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var dataInbound = GetQueryInventoryInbound(tenantId, request);
        var dataOutbound = GetQueryInventoryOutbound(tenantId, request);
        var queryOpening = GetQueryInventoryOpenning(tenantId, request);
        var inboundList = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);
        var outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);

        var results = new List<InOutStatementDto>();
        var data = await dataInbound.ToListAsync(cancellationToken);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var opening = await queryOpening.ToListAsync(cancellationToken);
        foreach (var item in opening)
        {
            var sku = await skuList.FirstOrDefaultAsync(x => x.Id == item.SkuId, cancellationToken);
            var unit = await uomList.FirstOrDefaultAsync(x => x.Id == item.SkuUomId, cancellationToken);
            results.Add(new InOutStatementDto
            {
                Description = "Nhập đầu kỳ",
                InwardQuantity = item.Quantity,
                SkuId = item.SkuId,
                ItemCode = sku?.sku_code ?? "",
                ItemName = sku?.sku_name ?? "",
                UnitId = item.SkuUomId ?? 0,
                Unit = unit?.UnitName ?? "",
                OutwardQuantity = 0,
                ReceiptDate = item.TransactionDate
            });
        }

        foreach (var item in data)
        {
            var receipt = await inboundList.FirstOrDefaultAsync(x => x.Id == item.InReceiptId, cancellationToken);
            if (receipt == null) continue;
            var sku = await skuList.FirstOrDefaultAsync(x => x.Id == item.SkuId, cancellationToken);
            var unit = await uomList.FirstOrDefaultAsync(x => x.Id == item.UnitId, cancellationToken);
            results.Add(new InOutStatementDto
            {
                ReceiptId = receipt.Id,
                Description = receipt.Description ?? "",
                InwardQuantity = item.InwardQuantity,
                ReceiptDate = receipt.CreateDate,
                SerialNumber = receipt.ReceiptNumber,
                SkuId = item.SkuId ?? 0,
                ItemCode = sku?.sku_code ?? "",
                ItemName = sku?.sku_name ?? "",
                UnitId = item.UnitId ?? 0,
                Unit = unit?.UnitName ?? ""
            });
        }

        var dt = await dataOutbound.ToListAsync(cancellationToken);
        foreach (var item in dt)
        {
            var receipt = await outboundList.FirstOrDefaultAsync(x => x.Id == item.OutReceiptId, cancellationToken);
            if (receipt == null) continue;
            var sku = await skuList.FirstOrDefaultAsync(x => x.Id == item.SkuId, cancellationToken);
            var unit = await uomList.FirstOrDefaultAsync(x => x.Id == item.UnitId, cancellationToken);
            results.Add(new InOutStatementDto
            {
                ReceiptId = receipt.Id,
                Description = receipt.Description ?? "",
                OutwardQuantity = item.OutwardQuantity,
                ReceiptDate = receipt.CreateDate,
                SerialNumber = receipt.ReceiptNumber,
                SkuId = item.SkuId ?? 0,
                ItemCode = sku?.sku_code ?? "",
                ItemName = sku?.sku_name ?? "",
                UnitId = item.UnitId ?? 0,
                Unit = unit?.UnitName ?? ""
            });
        }

        return results
            .OrderBy(x => x.ReceiptDate)
            .ThenBy(x => x.SerialNumber);
    }

    private IQueryable<StockTransactionEntity> GetQueryInventoryOpenning(long tenantId, InventoryReportRequest request)
    {
        var queryOpenning = _dbContext.GetDbSet<StockTransactionEntity>(tenantId);
        if (request.FromDate.HasValue)
        {
            queryOpenning = queryOpenning.Where(x => x.TransactionDate.Date < request.FromDate.Value.Date);
        }

        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        if (request.WarehouseId.GetValueOrDefault(0) > 0)
        {
            locationList = locationList.Where(x => x.WarehouseId == request.WarehouseId.GetValueOrDefault());
            var ids = locationList.Select(x => x.Id).Distinct();
            var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);
            var stockIds = stockList.Where(x => ids.Contains(x.goods_location_id))
                .Select(x => x.Id).ToList();
            queryOpenning = queryOpenning.Where(x => stockIds.Contains(x.StockId));
        }

        return queryOpenning;
    }

    private IQueryable<StockTransactionEntity> GetQueryInventoryClosed(long tenantId, InventoryReportRequest request)
    {
        var queryClosed = _dbContext.GetDbSet<StockTransactionEntity>(tenantId);
        if (request.ToDate.HasValue)
        {
            queryClosed = queryClosed.Where(x => x.TransactionDate.Date < request.ToDate.Value.Date);
        }

        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        if (request.WarehouseId.GetValueOrDefault(0) > 0)
        {
            locationList = locationList.Where(x => x.WarehouseId == request.WarehouseId.GetValueOrDefault());
            var ids = locationList.Select(x => x.Id).Distinct();
            var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);
            var stockIds = stockList.Where(x => ids.Contains(x.goods_location_id))
                .Select(x => x.Id).ToList();
            queryClosed = queryClosed.Where(x => stockIds.Contains(x.StockId));
        }

        return queryClosed;
    }

    private IQueryable<BaseInventoryCardItem> GetQueryInventoryOutbound(long tenantId,
        InventoryReportRequest request, List<ReceiptStatus>? receiptStatuses = null)
    {
        var outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);

        if (receiptStatuses?.Count > 0)
        {
            outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
                .Where(x => receiptStatuses.Contains(x.Status));
        }

        var outboundDetails = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
            .AsNoTracking();

        if (request.FromDate.HasValue)
        {
            outboundDetails = outboundDetails.Where(x => x.CreateDate.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            outboundDetails = outboundDetails.Where(x => x.CreateDate.Date <= request.ToDate.Value.Date);
        }

        var skuIds = (request.SkuIds ?? []).Where(id => id > 0);
        if (skuIds.Any())
        {
            outboundDetails = outboundDetails.Where(x => skuIds.Contains(x.SkuId));
        }

        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        if (request.WarehouseId.GetValueOrDefault(0) > 0)
        {
            locationList = locationList.Where(x => x.WarehouseId == request.WarehouseId.GetValueOrDefault());
            var ids = locationList.Select(x => x.Id).Distinct();
        }

        var locationIds = outboundDetails.Select(x => x.LocationId).Distinct().ToList();
        if (locationIds.Count != 0)
        {
            locationList = locationList.Where(x => locationIds.Contains(x.Id));
        }

        //ONLY get data completed
        var outreceiptIds = outboundList.Select(x => x.Id);

        outboundDetails = outboundDetails.Where(x => outreceiptIds.Contains(x.ReceiptId));
        var sIds = locationList.Select(x => x.Id).Distinct();
        return outboundDetails
            .Where(x => sIds.Contains(x.LocationId.GetValueOrDefault()))
            .Select(x => new BaseInventoryCardItem
            {
                DetailId = x.Id,
                SkuId = x.SkuId,
                UnitId = x.SkuUomId,
                TransactionDate = x.CreateDate,
                OutwardQuantity = x.Quantity,
                OutReceiptId = x.ReceiptId,
                LocationId = x.LocationId
            });
    }

    private IQueryable<BaseInventoryCardItem> GetQueryInventoryInbound(long tenantId,
        InventoryReportRequest request, List<ReceiptStatus>? receiptStatuses = null)
    {
        var inboundList = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(x => x.Status == ReceiptStatus.COMPLETE);

        if (receiptStatuses?.Count > 0)
        {
            inboundList = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
                .Where(x => receiptStatuses.Contains(x.Status));
        }

        var inboundDetails = _dbContext.GetDbSet<InboundReceiptDetailEntity>().AsNoTracking();

        if (request.FromDate.HasValue)
        {
            inboundDetails = inboundDetails.Where(x => x.CreateDate.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            inboundDetails = inboundDetails.Where(x => x.CreateDate.Date <= request.ToDate.Value.Date);
        }

        var skuIds = (request.SkuIds ?? []).Where(id => id > 0);
        if (skuIds.Any())
        {
            inboundDetails = inboundDetails.Where(x => skuIds.Contains(x.SkuId));
        }

        var locationList = _dbContext.GetDbSet<GoodslocationEntity>(tenantId);
        if (request.WarehouseId.GetValueOrDefault(0) > 0)
        {
            locationList = locationList.Where(x => x.WarehouseId == request.WarehouseId.GetValueOrDefault());
            var ids = locationList.Select(x => x.Id).Distinct();
        }

        var locationIds = inboundDetails.Select(x => x.LocationId).Distinct().ToList();
        if (locationIds.Count != 0)
        {
            locationList = locationList.Where(x => locationIds.Contains(x.Id));
        }

        //ONLY get data completed
        var receiptIds = inboundList.Select(x => x.Id);
        inboundDetails = inboundDetails.Where(x => receiptIds.Contains(x.ReceiptId));
        var sIds = locationList.Select(x => x.Id).Distinct();

        return inboundDetails
            .Where(x => sIds.Contains(x.LocationId.GetValueOrDefault()))
            .Select(x => new BaseInventoryCardItem
            {
                DetailId = x.Id,
                SkuId = x.SkuId,
                UnitId = x.SkuUomId,
                TransactionDate = x.CreateDate,
                InwardQuantity = x.Quantity,
                InReceiptId = x.ReceiptId,
                LocationId = x.LocationId
            });
    }

    /// <summary>
    /// Get Low Stock Alerts
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlerts(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId);
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>();
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId);
        var safetyStockList = _dbContext.GetDbSet<SkuSafetyStockEntity>();
        var inboundList = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId);
        var inboundDetails = _dbContext.GetDbSet<InboundReceiptDetailEntity>();

        var outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId);
        var outboundDetails = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>();

        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var uomLinkList = _dbContext.GetDbSet<SkuUomLinkEntity>()
            .AsNoTracking();

        var processingStatus = new List<ReceiptStatus>
        {
            ReceiptStatus.PROCESSING, ReceiptStatus.NEW
        };

        var results = new List<LowStockAlertDto>();
        var warehouses = await warehouseList.Where(x => x.is_valid).ToListAsync(cancellationToken);
        foreach (var warehouse in warehouses)
        {
            var safetyStocks = await safetyStockList
                .Where(x => x.WarehouseId == warehouse.Id)
                .Select(x => new LowStockAlertDto
                {
                    WarehouseId = x.WarehouseId,
                    SkuId = x.sku_id,
                    SafetyStockQty = x.safety_stock_qty,
                })
                .ToListAsync(cancellationToken);

            foreach (var safetyStock in safetyStocks)
            {
                var sku = await skuList.FirstOrDefaultAsync(x => x.Id == safetyStock.SkuId, cancellationToken);
                if (sku == null) continue;

                var stockQty = from stock in stockList
                               join loc in locationList on stock.goods_location_id equals loc.Id
                               where loc.WarehouseId == safetyStock.WarehouseId && stock.sku_id == safetyStock.SkuId
                               group stock by new { stock.sku_id, loc.WarehouseId } into sg
                               select new
                               {
                                   QtyTotal = sg.Sum(s => s.qty),
                               };

                var incomingStock = from inbound in inboundList
                                    join detail in inboundDetails on inbound.Id equals detail.ReceiptId
                                    where inbound.WarehouseId == safetyStock.WarehouseId && detail.SkuId == safetyStock.SkuId
                                    group detail by detail.SkuId into g
                                    select new
                                    {
                                        QtyTotal = g.Sum(s => s.Quantity)
                                    };

                var outcomingStock = from outbound in outboundList
                                     join detail in outboundDetails on outbound.Id equals detail.ReceiptId
                                     where outbound.WarehouseId == safetyStock.WarehouseId && detail.SkuId == safetyStock.SkuId
                                     group detail by detail.SkuId into g
                                     select new
                                     {
                                         QtyTotal = g.Sum(s => s.Quantity)
                                     };

                var incoming = await incomingStock.SumAsync(x => x.QtyTotal, cancellationToken);
                var outcoming = await outcomingStock.SumAsync(x => x.QtyTotal, cancellationToken);
                var curQty = await stockQty.SumAsync(x => x.QtyTotal, cancellationToken);

                if (curQty < safetyStock.SafetyStockQty || (curQty + incoming - outcoming) < safetyStock.SafetyStockQty)
                {
                    var uoms = await uomLinkList
                        .Where(x => x.SkuId == safetyStock.SkuId)
                        .ToListAsync(cancellationToken);

                    var unitId = 0;
                    if (uoms.Count == 1)
                    {
                        unitId = uoms.FirstOrDefault()?.SkuUomId ?? 0;
                    }
                    else if (uoms.Count > 0)
                    {
                        var baseUnit = uoms.FirstOrDefault(x => x.IsBaseUnit) ?? uoms.FirstOrDefault();
                        unitId = baseUnit?.SkuUomId ?? 0;
                    }

                    var unit = await uomList.FirstOrDefaultAsync(x => x.Id == unitId, cancellationToken);
                    safetyStock.UnitId = unitId;
                    safetyStock.Unit = unit?.UnitName ?? "Cái";
                    safetyStock.IncomeQuantity = incoming;
                    safetyStock.OutcomeQuantity = outcoming;
                    safetyStock.Quantity = curQty;
                    safetyStock.ItemCode = sku.sku_code ?? "";
                    safetyStock.ItemName = sku.sku_name ?? "";
                    safetyStock.WarehouseAddress = warehouse.address;
                    safetyStock.WarehouseName = warehouse.WarehouseName;

                    results.Add(safetyStock);
                }
            }
        }

        return results.OrderBy(x => x.WarehouseName).ThenBy(x => x.ItemName);
    }

    /// <summary>
    /// Report Outgoing Goods
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<ExportReportItem>> GetReportOutgoingGoods(InventoryReportRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var dataOutbound = GetQueryInventoryOutbound(tenantId, request);

        var outboundList = _dbContext.GetDbSet<OutBoundReceiptEntity>(tenantId)
            .Where(x => x.Status != ReceiptStatus.CANCELED);
        var outboundDetails = _dbContext.GetDbSet<OutBoundReceiptDetailEntity>()
            .AsNoTracking();
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId).Where(x => x.is_valid);
        var customerList = _dbContext.GetDbSet<CustomerEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var uomLinkList = _dbContext.GetDbSet<SkuUomLinkEntity>()
            .AsNoTracking();

        var results = new List<ExportReportItem>();

        var data = await dataOutbound.ToListAsync(cancellationToken);
        var items = await Convert2ReportItemsAsync(data,
            warehouseList, customerList, skuList, uomLinkList, uomList,
            outboundList, outboundDetails, cancellationToken);

        results.AddRange(items.OrderBy(x => x.ReceiptDate));
        var processingStatus = new List<ReceiptStatus>
        {
            ReceiptStatus.PROCESSING, ReceiptStatus.NEW
        };
        var processingOutbound = GetQueryInventoryOutbound(tenantId, request, processingStatus);

        var processing = await processingOutbound.ToListAsync(cancellationToken);
        items = await Convert2ReportItemsAsync(processing,
            warehouseList, customerList, skuList, uomLinkList, uomList,
            outboundList, outboundDetails, cancellationToken, true);

        results.AddRange(items.OrderBy(x => x.ReceiptDate));
        return results;
    }


    /// <summary>
    /// Report Incoming Goods
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<ImportReportItem>> GetReportIncomingGoods(InventoryReportRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var dataInbound = GetQueryInventoryInbound(tenantId, request);

        var inboundList = _dbContext.GetDbSet<InboundReceiptEntity>(tenantId)
            .Where(x => x.Status != ReceiptStatus.CANCELED);
        var inboundDetails = _dbContext.GetDbSet<InboundReceiptDetailEntity>()
            .AsNoTracking();
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId).Where(x => x.is_valid);
        var supplierList = _dbContext.GetDbSet<SupplierEntity>(tenantId);
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var uomLinkList = _dbContext.GetDbSet<SkuUomLinkEntity>()
            .AsNoTracking();

        var results = new List<ImportReportItem>();

        var data = await dataInbound.ToListAsync(cancellationToken);
        var items = await Convert2ReportIncomingItemsAsync(data,
            warehouseList, supplierList, skuList, uomLinkList, uomList,
            inboundList, inboundDetails, cancellationToken);

        results.AddRange(items.OrderBy(x => x.ReceiptDate));
        var processingStatus = new List<ReceiptStatus>
        {
            ReceiptStatus.PROCESSING, ReceiptStatus.NEW
        };
        var processingOutbound = GetQueryInventoryInbound(tenantId, request, processingStatus);

        var processing = await processingOutbound.ToListAsync(cancellationToken);
        items = await Convert2ReportIncomingItemsAsync(processing,
            warehouseList, supplierList, skuList, uomLinkList, uomList,
            inboundList, inboundDetails, cancellationToken, true);

        results.AddRange(items.OrderBy(x => x.ReceiptDate));
        return results;
    }

    private async Task<IEnumerable<ImportReportItem>> Convert2ReportIncomingItemsAsync(
        List<BaseInventoryCardItem> data, IQueryable<WarehouseEntity> warehouseList,
        IQueryable<SupplierEntity> supplierList, IQueryable<SkuEntity> skuList,
        IQueryable<SkuUomLinkEntity> uomLinkList, IQueryable<SkuUomEntity> uomList,
        IQueryable<InboundReceiptEntity> inboundList, IQueryable<InboundReceiptDetailEntity> inboundDetails,
        CancellationToken cancellationToken, bool isProcessing = false)
    {
        var results = new List<ImportReportItem>();
        foreach (var item in data)
        {
            var receipt = await inboundList
                .FirstOrDefaultAsync(x => x.Id == item.InReceiptId, cancellationToken);
            if (receipt == null) continue;

            var detail = await inboundDetails.FirstOrDefaultAsync(x => x.Id == item.DetailId, cancellationToken);
            if (detail == null) continue;

            var sku = await skuList.FirstOrDefaultAsync(x => x.Id == item.SkuId, cancellationToken);
            if (sku == null) continue;

            var uoms = await uomLinkList
                        .Where(x => x.SkuId == item.SkuId)
                        .ToListAsync(cancellationToken);

            var unitId = 0;
            if (uoms.Count == 1)
            {
                unitId = uoms.FirstOrDefault()?.SkuUomId ?? 0;
            }
            else if (uoms.Count > 0)
            {
                var baseUnit = uoms.FirstOrDefault(x => x.IsBaseUnit) ?? uoms.FirstOrDefault();
                unitId = baseUnit?.SkuUomId ?? 0;
            }

            var unit = await uomList.FirstOrDefaultAsync(x => x.Id == unitId, cancellationToken);
            var supplier = await supplierList.FirstOrDefaultAsync(x => x.Id == receipt.SupplierId, cancellationToken);
            var warehouse = await warehouseList.FirstOrDefaultAsync(x => x.Id == receipt.WarehouseId, cancellationToken);

            results.Add(new ImportReportItem
            {
                Unit = unit?.UnitName ?? "ĐVT",
                SupplierId = receipt.SupplierId,
                SupplierName = supplier?.supplier_name ?? "",
                Description = receipt.Description,
                ImportDate = item.TransactionDate,
                Quantity = isProcessing ? 0 : item.InwardQuantity,
                IncomingQty = isProcessing ? item.InwardQuantity : 0,
                SkuId = item.SkuId ?? 0,
                UnitId = item.UnitId ?? 0,
                ReceiptId = receipt.Id,
                ReceiptDate = receipt.CreateDate,
                WarehouseId = receipt.WarehouseId,
                SerialNumber = receipt.ReceiptNumber ?? "",
                ItemCode = sku.sku_code ?? "",
                ItemName = sku.sku_name ?? "",
                WarehouseAddress = warehouse?.address ?? "",
                WarehouseName = warehouse?.WarehouseName ?? ""
            });
        }

        return results;
    }

    private async Task<List<ExportReportItem>> Convert2ReportItemsAsync(List<BaseInventoryCardItem> data,
        IQueryable<WarehouseEntity> warehouseList,
        IQueryable<CustomerEntity> customerList,
        IQueryable<SkuEntity> skuList,
        IQueryable<SkuUomLinkEntity> uomLinkList,
        IQueryable<SkuUomEntity> uomList,
        IQueryable<OutBoundReceiptEntity> outboundList,
        IQueryable<OutBoundReceiptDetailEntity> outboundDetails,
        CancellationToken cancellationToken, bool isProcessing = false)
    {
        var results = new List<ExportReportItem>();
        foreach (var item in data)
        {
            var receipt = await outboundList
                .FirstOrDefaultAsync(x => x.Id == item.OutReceiptId, cancellationToken);
            if (receipt == null) continue;

            var detail = await outboundDetails.FirstOrDefaultAsync(x => x.Id == item.DetailId, cancellationToken);
            if (detail == null) continue;

            var sku = await skuList.FirstOrDefaultAsync(x => x.Id == item.SkuId, cancellationToken);
            if (sku == null) continue;

            var uoms = await uomLinkList
                        .Where(x => x.SkuId == item.SkuId)
                        .ToListAsync(cancellationToken);

            var unitId = 0;
            if (uoms.Count == 1)
            {
                unitId = uoms.FirstOrDefault()?.SkuUomId ?? 0;
            }
            else if (uoms.Count > 0)
            {
                var baseUnit = uoms.FirstOrDefault(x => x.IsBaseUnit) ?? uoms.FirstOrDefault();
                unitId = baseUnit?.SkuUomId ?? 0;
            }

            var unit = await uomList.FirstOrDefaultAsync(x => x.Id == unitId, cancellationToken);
            var customer = await customerList.FirstOrDefaultAsync(x => x.Id == receipt.CustomerId, cancellationToken);
            var warehouse = await warehouseList.FirstOrDefaultAsync(x => x.Id == receipt.WarehouseId, cancellationToken);

            results.Add(new ExportReportItem
            {
                Unit = unit?.UnitName ?? "ĐVT",
                CustomerId = receipt.CustomerId,
                CustomerName = customer?.customer_name ?? "",
                Description = receipt.Description,
                ExportDate = item.TransactionDate,
                Quantity = isProcessing ? 0 : item.OutwardQuantity,
                OutcomingQty = isProcessing ? item.OutwardQuantity : 0,
                SkuId = item.SkuId ?? 0,
                UnitId = item.UnitId ?? 0,
                ReceiptId = receipt.Id,
                ReceiptDate = receipt.ReceiptDate.GetValueOrDefault(receipt.CreateDate),
                WarehouseId = receipt.WarehouseId,
                SerialNumber = receipt.ReceiptNumber ?? "",
                ItemCode = sku.sku_code ?? "",
                ItemName = sku.sku_name ?? "",
                WarehouseAddress = warehouse?.address ?? "",
                WarehouseName = warehouse?.WarehouseName ?? ""
            });
        }

        return results;
    }

    /// <summary>
    /// Search Stock On Shelf
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<StockOnShelfDto>> SearchStockOnShelf(InventoryReportRequest request,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.tenant_id;
        var locationList = _dbContext.GetDbSet<GoodslocationEntity>()
            .AsNoTracking()
            .Where(x => x.IsValid);
        var warehouseList = _dbContext.GetDbSet<WarehouseEntity>(tenantId)
            .Where(x => x.is_valid);

        if (request.WarehouseId.GetValueOrDefault(0) > 0)
        {
            locationList = locationList.Where(x => x.WarehouseId == request.WarehouseId.GetValueOrDefault());
            //var ids = locationList.Select(x => x.Id).Distinct();
        }

        var locationIds = await locationList
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (locationIds.Count < 1) return [];
        var stockList = _dbContext.GetDbSet<StockEntity>(tenantId)
            .Where(x => x.actual_qty > 0);

        var processingStatus = new List<ReceiptStatus>
        {
            ReceiptStatus.PROCESSING, ReceiptStatus.NEW
        };
        var processingOutbound = GetQueryInventoryOutbound(tenantId, request, processingStatus);
        stockList = stockList.Where(x => locationIds.Contains(x.goods_location_id));
        var data = await stockList.ToListAsync(cancellationToken);

        if (data.Count < 1) return [];

        var locationWarehouseMap = await (
            from loc in locationList
            join wh in warehouseList on loc.WarehouseId equals wh.Id
            select new
            {
                loc.Id,
                loc.WarehouseId,
                wh.WarehouseName,
                loc.LocationName,
                loc.GoodsLocationType
            })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var results = new List<StockOnShelfDto>();
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var uomLinkList = _dbContext.GetDbSet<SkuUomLinkEntity>()
            .AsNoTracking();

        foreach (var stockQty in data)
        {
            if (stockQty == null) continue;
            var sku = await skuList.FirstOrDefaultAsync(x => x.Id == stockQty.sku_id, cancellationToken);
            if (sku == null) continue;

            var outcomingStock = processingOutbound
                .Where(x => x.SkuId == stockQty.sku_id && x.LocationId == stockQty.goods_location_id)
                .Sum(x => x.OutwardQuantity);

            if (!locationWarehouseMap.TryGetValue(stockQty.goods_location_id, out var lw))
                continue;

            var uoms = await uomLinkList
                        .Where(x => x.SkuId == stockQty.sku_id)
                        .ToListAsync(cancellationToken);

            var unitId = 0;
            if (uoms.Count == 1)
            {
                unitId = uoms.FirstOrDefault()?.SkuUomId ?? 0;
            }
            else if (uoms.Count > 0)
            {
                var baseUnit = uoms.FirstOrDefault(x => x.IsBaseUnit) ?? uoms.FirstOrDefault();
                unitId = baseUnit?.SkuUomId ?? 0;
            }
            var isVirtual = (lw?.GoodsLocationType ?? GoodsLocationTypeEnum.StorageSlot) == GoodsLocationTypeEnum.VirtualLocation;

            var unit = await uomList.FirstOrDefaultAsync(x => x.Id == unitId, cancellationToken);

            results.Add(new StockOnShelfDto
            {
                Quantity = stockQty.actual_qty,
                AvailableQty = stockQty.actual_qty - outcomingStock,
                LocationId = stockQty.goods_location_id,
                ExpiryDate = stockQty.expiry_date,
                PalletName = isVirtual ? "" : stockQty.Palletcode,
                SkuId = stockQty.sku_id,
                UnitId = unitId,
                Unit = unit?.UnitName ?? "Cái",
                ItemCode = sku.sku_code,
                ItemName = sku.sku_name,
                LocationName = lw?.LocationName ?? string.Empty,
                WarehouseName = lw?.WarehouseName ?? string.Empty,
                WarehouseId = lw?.WarehouseId ?? 0,
            });
        }

        return results.OrderBy(x => x.WarehouseName)
            .ThenBy(x => x.LocationName);
    }

    /// <summary>
    /// Get Vendors
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<VendorMaster>> GetVendors(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var curUserRole = currentUser.user_role;
        bool isSysAdmin = UserRoleDef.IsSystemAdministrator(curUserRole);
        if (curUserRole != UserRoleDef.ShowVendors && !isSysAdmin)
        {
            bool isAdmin = UserRoleDef.IsAdminRole(curUserRole);
            if (!isAdmin) throw new Exception("You don't have permission");
        }       

        var fromDate = DateTime.UtcNow.AddMonths(-1).Date;
        var toDate = DateTime.UtcNow.AddMonths(1).Date;

        var query = _dbContext.GetDbSet<TenantEntity>().Where(x => x.ValidTo.HasValue);
        query = query.Where(x => x.ValidTo >= fromDate && x.ValidTo <= toDate);

        return await query.Select(x => new VendorMaster
        {
            Id = x.Id,
            Company = x.TenantName,
            ContactName = x.DisplayName,
            ContactNumber = x.ContactNumber,
            CreatedDate = x.CreatedDate,
            ValidTo = x.ValidTo.GetValueOrDefault()
        })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Ban Vendors
    /// </summary>
    /// <param name="vendorId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> BanVendors(int vendorId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var curUserRole = currentUser.user_role;
        bool isAdmin = UserRoleDef.IsAdminRole(curUserRole);

        if (!isAdmin) throw new Exception("You don't have permission");

        var query = _dbContext.GetDbSet<TenantEntity>().Where(x => x.Id == vendorId);
        var entity = await query.FirstOrDefaultAsync(cancellationToken);

        if(entity != null)
        {
            var obj = new
            {
                entity.TenantName,
                entity.DisplayName,
                entity.ValidTo,
                entity.CreatedDate,
                entity.ContactNumber,
            };

            string jsonString = JsonConvert.SerializeObject(obj);
            await _actionLogService.AddLogAsync($"[Ban Vendors] vendor {jsonString}",
                "Vendors", currentUser);

            entity.ValidTo = DateTime.UtcNow.AddMonths(-3);
            return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        return false;
    }
}
