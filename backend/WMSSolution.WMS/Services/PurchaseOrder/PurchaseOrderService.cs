using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WMSSolution.Core;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Core.Utility;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Enums.Inbound;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.PurchaseOrders;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels.PurchaseOrders;
using WMSSolution.WMS.IServices.ActionLog;
using WMSSolution.WMS.IServices.PurchaseOrder;

namespace WMSSolution.WMS.Services.PurchaseOrder;

/// <summary>
/// Purchase Order Service
/// </summary>
/// <remarks>
/// Purchase Order Service constructor
/// </remarks>
/// <param name="dbContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
/// <param name="functionHelper">Function Helper</param>
/// <param name="logger">Logger</param>
/// <param name="actionLogService">Action Log Service</param>
public class PurchaseOrderService(
    SqlDBContext dbContext,
    IStringLocalizer<MultiLanguage> stringLocalizer,
    FunctionHelper functionHelper,
    ILogger<PurchaseOrderService> logger, IActionLogService actionLogService) :
    BaseService<PurchaseOrderEntity>, IPurchaseOrderService
{
    #region Args

    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext = dbContext;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Function Helper
    /// </summary>
    private readonly FunctionHelper _functionHelper = functionHelper;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<PurchaseOrderService> _logger = logger;

    /// <summary>
    /// Action Log Service
    /// </summary>
    private readonly IActionLogService _actionLogService = actionLogService;

    #endregion

    #region Api

    /// <summary>
    /// Get purchase order page list
    /// Input: PageSearch - Search parameters with pagination
    /// Output: (List of PageSearchPOResponse, int total) - Purchase orders list and total count
    /// Process: Apply filters, join with details, calculate totals, paginate results
    /// </summary>
    /// <param name="pageSearch">Page search parameters</param>
    /// <param name="currentUser">Current user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of purchase orders and total count</returns>
    public async Task<(List<PageSearchPOResponse> data, int totals)> GetPageAsync(
        PageSearch pageSearch,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count > 0)
        {
            pageSearch.searchObjects.ForEach(s => queries.Add(s));
        }

        var poQuery = _dbContext.GetDbSet<PurchaseOrderEntity>(currentUser.tenant_id)
            .AsNoTracking()
            .Where(queries.AsGroupedExpression<PurchaseOrderEntity>());


        var totals = await poQuery.CountAsync(cancellationToken);

        var rows = await poQuery
            .OrderBy(x => x.PoStatus)
            .ThenBy(x => x.ExpectedDeliveryDate)
            .ThenByDescending(x => x.OrderDate)
            .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
            .Take(pageSearch.pageSize)
            .Select(x => new PageSearchPOResponse
            {
                Id = x.Id,
                PoNo = x.PoNo,
                ExpectedDeliveryDate = x.ExpectedDeliveryDate,
                PoStatus = (int)x.PoStatus,
                TotalAmount = x.TotalAmount ?? 0m,
                Creator = x.Creator,
                OrderDate = x.OrderDate,
            })
            .ToListAsync(cancellationToken);

        return (rows, totals);
    }

    /// <summary>
    /// Update purchase order
    /// Input: CreateNewOrderRequest - Updated purchase order data
    /// Output: (bool flag, string message) - Success status and message
    /// Process: Validate, update order and details, save changes
    /// Note: PO number can only be modified when status = 0 (Created)
    /// </summary>
    /// <param name="request">Purchase order request</param>
    /// <param name="currentUser">Current user</param>
    /// <returns>Success flag and message</returns>
    public async Task<(bool, string)> UpdateAsync(CreateNewOrderRequest request, CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<PurchaseOrderEntity>();
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == request.id);

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        if (request.expected_delivery_date.IsExpired())
        {
            return (false, _stringLocalizer["expected_delivery_date_invalid"]);
        }

        // Validate: PO number cannot be changed after status changes from Created (0)
        if (entity.PoStatus != PoStatusEnum.CREATED && request.po_no != entity.PoNo)
        {
            return (false, "Cannot modify PO number when status is not 'Created'");
        }

        // Update main order
        entity.SupplierId = request.supplier_id;
        entity.SupplierName = request.supplier_name;
        entity.ExpectedDeliveryDate = request.expected_delivery_date;

        // Only allow status update if not downgrading from higher status
        if (request.po_status >= (int)entity.PoStatus)
        {
            entity.PoStatus = (PoStatusEnum)request.po_status;
        }

        // Update PO number only if status is still Created (0)
        if (entity.PoStatus == (int)PoStatusEnum.CREATED && !string.IsNullOrEmpty(request.po_no))
        {
            entity.PoNo = request.po_no;
        }

        // Update details if provided
        if (request.Details != null && request.Details.Count != 0)
        {
            var detailsDbSet = _dbContext.GetDbSet<PurchaseOrderDetailsEntity>();
            var existingDetails = await detailsDbSet.Where(d => d.PoId == request.id).ToListAsync();

            // Remove old details
            detailsDbSet.RemoveRange(existingDetails);

            // Add new details
            var newDetails = request.Details.Select(d => new PurchaseOrderDetailsEntity
            {
                PoId = entity.Id,
                SkuId = d.sku_id,
                SkuName = d.sku_name,
                QtyOrdered = d.qty_ordered,
                QtyReceived = 0,
                UnitPrice = d.unit_price
            });

            await detailsDbSet.AddRangeAsync(newDetails);
        }

        await _dbContext.SaveChangesAsync();
        return (true, _stringLocalizer["save_success"]);
    }

    /// <summary>
    /// Delete purchase order
    /// Input: int id - Purchase order id
    /// Output: (bool flag, string message) - Success status and message
    /// Process: Soft delete order and details by setting is_valid to false
    /// </summary>
    /// <param name="id">Purchase order id</param>
    /// <param name="cancellationToken"></param>
    /// <param name="currentUser">Current user</param>
    /// <returns>Success flag and message</returns>
    public async Task<(bool, string)> DeleteAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {

        var entity = await _dbContext.GetDbSet<PurchaseOrderEntity>(currentUser.tenant_id)
                                     .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        // Delete the order (hard delete or mark as canceled)
        entity.PoStatus = PoStatusEnum.CANCELED;

        //// Also delete/cancel details
        //var detailsDbSet = _dbContext.GetDbSet<PurchaseOrderDetailsEntity>();
        //var details = await detailsDbSet.Where(d => d.PoId == id).ToListAsync();

        //// Remove details
        //detailsDbSet.RemoveRange(details);

        await _dbContext.SaveChangesAsync();
        return (true, _stringLocalizer["delete_success"]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<List<PurchaseOrderEntity>> GetOpenPosAsync(CurrentUser currentUser)
    {
        try
        {
            var dbSet = _dbContext.GetDbSet<PurchaseOrderEntity>();
            var results = await dbSet.AsNoTracking()
                .Include(p => p.Details)
                .Where(p =>
                    (p.PoStatus == PoStatusEnum.CREATED ||
                     p.PoStatus == PoStatusEnum.IN_PROGRESS)
                    &&
                    p.Details.Any(d => d.QtyOrdered > d.QtyReceived)
                )
                .OrderByDescending(p => p.OrderDate)
                .ToListAsync();
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetOpenPosAsync");
            throw;
        }
    }

    /// <summary>
    /// Close purchase order (Short Close / Cancellation)
    /// Input: int id - Purchase order id
    /// Output: (bool success, string message) - Success status and message
    /// Process: 
    /// 1. Validate PO exists and status is CREATED or IN_PROGRESS
    /// 2. Check for pending ASNs linked to this PO
    /// 3. Determine target status (CANCELLED if no items received, COMPLETED otherwise)
    /// 4. Update PO status
    /// </summary>
    /// <param name="id">Purchase order id</param>
    /// <param name="currentUser">Current user</param>
    /// <returns>Success flag and message</returns>
    public async Task<(bool success, string message)> CloseAsync(int id, CurrentUser currentUser)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // dbsets   
            var asnMasterDbSet = _dbContext.GetDbSet<AsnmasterEntity>();
            var asnDbSet = _dbContext.GetDbSet<AsnEntity>();
            var poDbSet = _dbContext.GetDbSet<PurchaseOrderEntity>();
            var entity = await poDbSet.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return (false, _stringLocalizer["Not Exists"]);
            }

            // Validate: Only CREATED (0) or IN_PROGRESS (1) can be closed
            if (entity.PoStatus != PoStatusEnum.CREATED &&
                entity.PoStatus != PoStatusEnum.IN_PROGRESS)
            {
                return (false, _stringLocalizer["Po Already Closed"]);
            }

            // Check for pending ASNs linked to this PO
            var linkedAsnIds = await asnMasterDbSet
                .Where(m => m.po_id == id)
                .Select(m => m.Id)
                .ToListAsync();

            if (linkedAsnIds.Count != 0)
            {
                var hasActiveOperations = await asnDbSet
                    .AnyAsync(d => linkedAsnIds.Contains(d.asnmaster_id) &&
                           (d.asn_status == (byte)AsnStatusEnum.PREPARE_UNLOAD ||
                            d.asn_status == (byte)AsnStatusEnum.PREPARE_QC ||
                            d.asn_status == (byte)AsnStatusEnum.PREPARE_PUTAWAY ||
                            d.asn_status == (byte)AsnStatusEnum.WAITING_ROBOT));

                if (hasActiveOperations)
                {
                    return (false, _stringLocalizer["PO can't close because has value in ASN"]);
                }

                var pendingDetails = await asnDbSet
                                        .Where(d => linkedAsnIds.Contains(d.asnmaster_id) &&
                                         d.asn_status == (byte)AsnStatusEnum.PREPARE_COMING)
                                         .ToListAsync();

                if (pendingDetails.Count != 0)
                {
                    foreach (var item in pendingDetails)
                    {
                        item.asn_status = (byte)AsnStatusEnum.CANCELED;
                        item.last_update_time = DateTime.UtcNow;
                    }
                }

                var mastersToCancel = await asnMasterDbSet
                    .Where(m => linkedAsnIds.Contains(m.Id) &&
                                m.asn_status == (byte)AsnMasterStatusEnum.CREATED)
                    .ToListAsync();

                if (mastersToCancel.Count != 0)
                {
                    foreach (var m in mastersToCancel)
                    {
                        m.asn_status = (byte)AsnMasterStatusEnum.CANCELED;
                        m.last_update_time = DateTime.UtcNow;
                    }
                }
            }

            var totalQtyReceived = await _dbContext
                .GetDbSet<PurchaseOrderDetailsEntity>()
                .Where(d => d.PoId == id)
                .SumAsync(d => d.QtyReceived);

            // Determine target status
            int oldStatus = (int)entity.PoStatus;
            entity.PoStatus = PoStatusEnum.COMPLETED;
            entity.LastUpdateTime = DateTime.UtcNow;

            var result = await _dbContext.SaveChangesAsync();
            if (result > 0)
            {
                await transaction.CommitAsync();
                _logger.LogInformation("PO {PoId} force closed successfully.", id);
                return (true, _stringLocalizer["Success"]);
            }
            else
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("CloseAsync failed. SaveChangesAsync returned 0 for PO {PoId}", id);
                return (false, _stringLocalizer["Save Failed"]);
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "CloseAsync exception for PO {PoId}", id);
            return (false, _stringLocalizer["Save Failed"]);
        }
    }

    /// <summary>
    /// Create New Order
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(int, string)> CreateNewPoOrder(CreateNewPoRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return (0, "Request cannot be null");
        }

        if (request.Details == null || request.Details.Count == 0)
        {
            return (0, "Details cannot be empty");
        }

        if (request.OrderDate == default)
        {
            return (0, "Order date is required");
        }

        if (request.Details.Any(x => x.SkuId <= 0))
        {
            return (0, "Sku is required");
        }

        if (request.Details.Any(x => x.Qty <= 0))
        {
            return (0, "Qty must be positive");
        }

        var existingPo = await _dbContext.GetDbSet<PurchaseOrderEntity>(currentUser.tenant_id)
            .FirstOrDefaultAsync(p => p.PoNo == request.PoNo, cancellationToken);

        if (existingPo is not null)
        {
            return (0, "PO number already exists");
        }

        //var duplicateLines = request.Details
        //    .GroupBy(x => new
        //    {
        //        x.SupplierId,
        //        x.SkuId,
        //        ExpiryDate = x.ExpiryDate?.Date
        //    })
        //    .Where(g => g.Count() > 1)
        //    .ToList();

        //if (duplicateLines.Count != 0)
        //{
        //    return (0, _stringLocalizer["po_details_duplicate"]);
        //}

        var firstDetail = request.Details.First();
        var distinctSupplierIds = request.Details
          .Select(x => x.SupplierId)
          .Distinct()
          .ToList();

        var isMultiSupplier = distinctSupplierIds.Count > 1;

        var details = request.Details.Select(d => new PurchaseOrderDetailsEntity
        {
            SkuId = d.SkuId,
            SkuName = d.SkuName,
            SupplierId = d.SupplierId,
            QtyOrdered = d.Qty,
            UnitPrice = d.UnitPrice,
            SkuUomId = d.SkuUomId,
            ExpiryDate = d.ExpiryDate,
        }).ToList();


        var lineTotal = details.Sum(x => x.QtyOrdered * (x.UnitPrice ?? 0));
        var shipping = request.ShippingAmount ?? 0;

        var entity = new PurchaseOrderEntity
        {
            PoNo = request.PoNo,
            SupplierId = firstDetail.SupplierId,
            SupplierName = request.SupplierName,
            IsMultiSupplier = isMultiSupplier,

            OrderDate = request.OrderDate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            PoStatus = PoStatusEnum.CREATED,

            CreateTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow,
            Creator = currentUser.user_name,

            BuyerName = request.BuyerName,
            BuyerAddress = request.BuyerAddress,
            PaymentTerm = request.PaymentTerm,
            Description = request.Description,

            ShippingAmount = request.ShippingAmount,
            TotalAmount = lineTotal + shipping,

            Details = details
        };


        await _dbContext.GetDbSet<PurchaseOrderEntity>().AddAsync(entity, cancellationToken);
        var affected = await _dbContext.SaveChangesAsync(cancellationToken);

        if (affected <= 0)
        {
            return (0, _stringLocalizer["save_failed"]);
        }

        return (entity.Id, _stringLocalizer["save_success"]);
    }

    /// <summary>
    /// Generate PO number
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GeneratePoNoAsync(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var poNo = await _functionHelper.GetFormNoAsync("purchaseorder", "PO");
        return poNo;
    }

    /// <summary>
    /// Get detail async
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PoDetailResponseDto?> GetDetailAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var po = await _dbContext.GetDbSet<PurchaseOrderEntity>(currentUser.tenant_id)
         .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (po == null) return null;

        var detailRows = await _dbContext.GetDbSet<PurchaseOrderDetailsEntity>()
            .AsNoTracking()
            .Where(x => x.PoId == id)
            .ToListAsync(cancellationToken);

        var skuIds = detailRows
            .Select(x => x.SkuId)
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var supplierIds = detailRows
            .Select(x => x.SupplierId ?? 0)
            .Where(x => x > 0)
            .Append(po.SupplierId ?? 0)
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var skuUomIds = detailRows
                          .Select(x => x.SkuUomId)
                          .Where(x => x > 0)
                          .Distinct()
                          .ToList();

        var skuMap = await _dbContext.GetDbSet<SkuEntity>(currentUser.tenant_id)
            .AsNoTracking()
            .Where(x => skuIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var supplierMap = await _dbContext.GetDbSet<SupplierEntity>(currentUser.tenant_id)
            .AsNoTracking()
            .Where(x => supplierIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);


        var validSkuUomLinks = await _dbContext.GetDbSet<SkuUomLinkEntity>()
            .AsNoTracking()
            .Where(x => skuIds.Contains(x.SkuId) && skuUomIds.Contains(x.SkuUomId))
            .Select(x => new { x.SkuId, x.SkuUomId })
            .ToListAsync(cancellationToken);

        var validSkuUomLinkSet = validSkuUomLinks
            .Select(x => (x.SkuId, x.SkuUomId))
            .ToHashSet();

        var uomMap = await _dbContext.GetDbSet<SkuUomEntity>(currentUser.tenant_id)
            .AsNoTracking()
            .Where(x => skuUomIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var poSupplierName = (po.SupplierId.HasValue && supplierMap.TryGetValue(po.SupplierId.Value, out var poSupplier))
            ? poSupplier.supplier_name
            : po.SupplierName;

        return new PoDetailResponseDto
        {
            Id = po.Id,
            PoNo = po.PoNo,
            SupplierId = po.SupplierId,
            SupplierName = poSupplierName,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            PoStatus = (int)po.PoStatus,
            BuyerName = po.BuyerName,
            BuyerAddress = po.BuyerAddress,
            PaymentTerm = po.PaymentTerm,
            Description = po.Description,
            ShippingAmount = po.ShippingAmount,
            TotalAmount = po.TotalAmount,
            Details = detailRows.Select(d =>
            {
                skuMap.TryGetValue(d.SkuId, out var sku);

                var detailSupplierId = d.SupplierId ?? po.SupplierId;
                string? detailSupplierName = null;

                if (detailSupplierId.HasValue && supplierMap.TryGetValue(detailSupplierId.Value, out var supplier))
                {
                    detailSupplierName = supplier.supplier_name;
                }
                else
                {
                    detailSupplierName = d.SupplierName ?? poSupplierName;
                }

                string? unitName = null;
                if (d.SkuId > 0 && d.SkuUomId > 0
                    && validSkuUomLinkSet.Contains((d.SkuId, d.SkuUomId))
                    && uomMap.TryGetValue(d.SkuUomId, out var uom))
                {
                    unitName = uom.UnitName;
                }


                return new PoDetailItemDto
                {
                    Id = d.Id,
                    SkuId = d.SkuId,
                    SkuCode = sku?.sku_code ?? d.SkuId.ToString(),
                    SkuName = sku?.sku_name ?? d.SkuName,
                    QtyOrdered = d.QtyOrdered,
                    QtyReceived = d.QtyReceived,
                    UnitPrice = d.UnitPrice,
                    ExpiryDate = d.ExpiryDate,
                    SupplierId = detailSupplierId,
                    SupplierName = detailSupplierName,
                    SkuUomId = d.SkuUomId,
                    UnitName = unitName
                };
            }).ToList()
        };
    }

    #endregion
}
