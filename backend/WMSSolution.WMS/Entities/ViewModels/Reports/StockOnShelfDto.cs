
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Stock On Shelf
/// </summary>
public class StockOnShelfDto
{
    /// <summary>
    /// Warehouse id 
    /// </summary>
    public int? WarehouseId { get; set; }

    /// <summary>
    /// Warehouse name 
    /// </summary>
    public string? WarehouseName { get; set; }
    /// <summary>
    /// Location Id
    /// </summary>
    public int? LocationId { get; set; }
    /// <summary>
    /// Location Name
    /// </summary>
    public string LocationName { get; set; } = "";
    /// <summary>
    /// Sku Id
    /// </summary>
    public int SkuId { get; set; }
    /// <summary>
    /// Unit Id
    /// </summary>
    public int UnitId { get; set; }
    /// <summary>
    /// Item Code
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;
    /// <summary>
    /// Item Name
    /// </summary>
    public string ItemName { get; set; } = string.Empty;
    /// <summary>
    /// Unit
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>
    /// Quantity
    /// </summary>
    public decimal? Quantity { get; set; } = 0;
    /// <summary>
    /// Available Qty
    /// </summary>
    public decimal? AvailableQty { get; set; } = 0;
    /// <summary>
    /// Expiry Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; } = "";
    /// <summary>
    /// Pallet Id
    /// </summary>
    public int? PalletId { get; set; }
    /// <summary>
    /// Pallet Name
    /// </summary>
    public string? PalletName { get; set; } = "";
}