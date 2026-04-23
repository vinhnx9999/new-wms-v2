namespace WMSSolution.WMS.Entities.ViewModels.Warehouse;

/// <summary>
/// Sku Safety Stock
/// </summary>
public class SkuSafetyStockDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; } = 0;
    /// <summary>
    /// Sku Id
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// Sku Name
    /// </summary>
    public string? SkuName { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string? SkuCode { get; set; }
    /// <summary>
    /// Safety Stock Qty
    /// </summary>
    public int? SafetyStockQty { get; set; }
    /// <summary>
    /// Warehouse Id
    /// </summary>
    public int? WarehouseId { get; set; }
    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string? WarehouseName { get; set; }
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string? WarehouseAddress { get; set; }
}
