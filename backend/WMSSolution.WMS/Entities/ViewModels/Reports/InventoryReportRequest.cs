
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Inventory Report Request
/// </summary>
public class InventoryReportRequest
{
    /// <summary>
    /// From Date
    /// </summary>
    public DateTime? FromDate { get; set; }
    /// <summary>
    /// To Date
    /// </summary>
    public DateTime? ToDate { get; set; }
    /// <summary>
    /// Warehouse Id
    /// </summary>
    public int? WarehouseId { get; set; }
    /// <summary>
    /// Sku Ids
    /// </summary>
    public IEnumerable<int> SkuIds { get; set; } = [];
}