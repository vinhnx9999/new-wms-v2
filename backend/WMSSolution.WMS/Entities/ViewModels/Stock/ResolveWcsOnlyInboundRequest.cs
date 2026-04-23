namespace WMSSolution.WMS.Entities.ViewModels.Stock;

/// <summary>
/// Resolve Request
/// </summary>
public class ResolveWcsOnlyInboundRequest
{
    /// <summary>
    /// Warehouse
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// pallet code
    /// </summary>
    public string PalletCode { get; set; } = string.Empty;

    /// <summary>
    /// Location name 
    /// </summary>
    public string WcsLocation { get; set; } = string.Empty;

    /// <summary>
    /// Update note 
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// item insert 
    /// </summary>
    public List<ResolveWcsOnlyInboundItemRequest> Items { get; set; } = [];
}

/// <summary>
/// Item
/// </summary>
public class ResolveWcsOnlyInboundItemRequest
{
    /// <summary>
    /// Sku
    /// </summary>
    public int SkuId { get; set; }

    /// <summary>
    /// qty
    /// </summary>
    public int Qty { get; set; }

    /// <summary>
    /// Supplier Id 
    /// </summary>
    /// 
    public int? SupplierId { get; set; }

    /// <summary>
    /// Expiry date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// price
    /// </summary>
    public decimal? Price { get; set; }
}