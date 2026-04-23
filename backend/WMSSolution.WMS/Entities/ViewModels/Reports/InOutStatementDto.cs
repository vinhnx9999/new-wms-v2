
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Inventory In-Out Statement
/// </summary>
public class InOutStatementDto
{
    /// <summary>
    /// Receipt Id
    /// </summary>
    public int? ReceiptId { get; set; }
    /// <summary>
    /// Receipt Date
    /// </summary>
    public DateTime? ReceiptDate { get; set; }
    /// <summary>
    /// Serial Number
    /// </summary>
    public string SerialNumber { get; set; } = "";
    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = "";
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
    /// Unit
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>
    /// Inward Quantity
    /// </summary>
    public decimal? InwardQuantity { get; set; } = 0;
    /// <summary>
    /// Outward Quantity
    /// </summary>
    public decimal? OutwardQuantity { get; set; } = 0;

}
