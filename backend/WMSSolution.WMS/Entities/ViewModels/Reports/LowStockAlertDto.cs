
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Low Stock Alert
/// </summary>
public class LowStockAlertDto
{
    /// <summary>
    /// Warehouse Id
    /// </summary>
    public int WarehouseId { get; set; }
    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string WarehouseAddress { get; set; } = string.Empty;
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; }
    /// <summary>
    /// UnitId
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
    /// Unit of measure
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>
    /// Safety Stock Qty
    /// </summary>
    public decimal SafetyStockQty { get; set; } = 0;
    /// <summary>
    /// Quantity in stock
    /// </summary>
    public decimal Quantity { get; set; } = 0;

    /// <summary>
    /// Inward Quantity
    /// </summary>
    public decimal IncomeQuantity { get; set; } = 0;
    /// <summary>
    /// Outward Quantity
    /// </summary>
    public decimal OutcomeQuantity { get; set; } = 0;
}
