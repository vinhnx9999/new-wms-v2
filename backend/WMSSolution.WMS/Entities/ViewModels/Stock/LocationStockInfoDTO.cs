namespace WMSSolution.WMS.Entities.ViewModels.Stock;

/// <summary>
/// Location stock DTO
/// </summary>
public class LocationStockInfoDTO
{
    /// <summary>
    /// PalletCode 
    /// </summary>
    public string PalletCode { get; set; } = default!;

    /// <summary>
    /// Location Id
    /// </summary>
    public int LocationId { get; set; } = default!;
    /// <summary>
    /// Location Name
    /// </summary>
    public string LocationName { get; set; } = default!;

    /// <summary>
    /// Putaway Date
    /// </summary>
    public DateTime? PutawayDate { get; set; }

    /// <summary>
    /// Items
    /// </summary>
    public List<LocationStockInfoItemDTO>? Items { get; set; }
}

/// <summary>
/// Item of info
/// </summary>
public class LocationStockInfoItemDTO
{
    /// <summary>
    /// Sku id 
    /// </summary>
    public int SkuId { get; set; } = default!;

    /// <summary>
    /// Sku name
    /// </summary>
    public string SkuName { get; set; } = default!;

    /// <summary>
    /// Sku code 
    /// </summary>
    public string SkuCode { get; set; } = default!;

    /// <summary>
    /// quantity
    /// </summary>
    public int Quantity { get; set; } = default!;

    /// <summary>
    /// expiry date 
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// supplier Id 
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Supplier Name 
    /// </summary>
    public string? SupplierName { get; set; }
}
