
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Inventory Report Item
/// </summary>
public class InventoryReportItem
{
    /// <summary>
    /// 
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// SkuId
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// UnitId
    /// </summary>
    public int? UnitId { get; set; }
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
    /// Opening Balance
    /// </summary>
    public decimal OpeningBalance { get; set; } = 0;
    /// <summary>
    /// Inward Quantity
    /// </summary>
    public decimal InwardQuantity { get; set; } = 0;
    /// <summary>
    /// Outward Quantity
    /// </summary>
    public decimal OutwardQuantity { get; set; } = 0;

    /// <summary>
    /// Opening Cost
    /// </summary>
    public decimal OpeningCost { get; set; } = 0;
    /// <summary>
    /// Inward Cost
    /// </summary>
    public decimal InwardCost { get; set; } = 0;
    /// <summary>
    /// Outward Cost
    /// </summary>
    public decimal OutwardCost { get; set; } = 0;
    /// <summary>
    /// Location Id
    /// </summary>
    public int LocationId { get; set; }
    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }
}
