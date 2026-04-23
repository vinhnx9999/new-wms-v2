namespace WMSSolution.WMS.Services.Warehouse.ManagementPrinciples.Models;

/// <summary>
/// InboundGoods
/// </summary>
public class InboundGoods
{
    /// <summary>
    /// Warehouse Id
    /// </summary>
    public int WarehouseId { get; set; }
    /// <summary>
    /// PurchaseOrderDate
    /// </summary>
    public DateTime PurchaseOrderDate { get; set; } = DateTime.UtcNow.AddDays(-3);
    /// <summary>
    /// ExpectedDeliveryDate
    /// </summary>
    public DateTime ExpectedDeliveryDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// ReceivingDate
    /// </summary>
    public DateTime ReceivingDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Details
    /// </summary>
    public IEnumerable<InboundDetail> Details { get; set; } = [];
}

/// <summary>
/// InboundDetail
/// </summary>
public class InboundDetail
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; } = 1;
    /// <summary>
    /// SupplierId
    /// </summary>
    public int SupplierId { get; set; } = 1;
    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; } = 0;
    /// <summary>
    /// ExpirationDate
    /// </summary>
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(90);
}
