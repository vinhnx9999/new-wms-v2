namespace WMSSolution.WMS.Entities.ViewModels.Stock;

/// <summary>
/// Merger Conflict 
/// </summary>
public class ResolvePalletMergeSameLocationRequest
{
    /// <summary>
    /// warehouse
    /// </summary>
    public int WarehouseId { get; set; } = default!;

    /// <summary>
    /// Location name
    /// </summary>
    public string LocationName { get; set; } = default!;

    /// <summary>
    /// Wwms pallet code
    /// </summary>
    public string? WmsPalletCode { get; set; }
    /// <summary>
    /// wcs pallet code
    /// </summary>
    public string? WcsPalletCode { get; set; }

    /// <summary>
    /// Target pallet code
    /// </summary>
    public string TargetPalletCode { get; set; } = default!;

    /// <summary>
    /// Note
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// item
    /// </summary>
    public List<ResolvePalletMergeSameLocationItemRequest> Items { get; set; } = [];
}

/// <summary>
/// Items
/// </summary>
public class ResolvePalletMergeSameLocationItemRequest
{

    /// <summary>
    /// Sku Id 
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
    /// <summary>
    /// Expiry date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Qty
    /// </summary>
    public int? Qty { get; set; }
}