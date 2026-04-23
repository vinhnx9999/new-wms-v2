
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Normal Report Item
/// </summary>
public class NormalReportItem
{
    /// <summary>
    /// WarehouseId
    /// </summary>
    public int WarehouseId { get; set; }
    /// <summary>
    /// WarehouseName
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string WarehouseAddress { get; set; } = string.Empty;
    /// <summary>
    /// ReceiptId
    /// </summary>
    public int? ReceiptId { get; set; }
    /// <summary>
    /// Serial Number
    /// </summary>
    public string SerialNumber { get; set; } = "";
    /// <summary>
    /// Receipt Date
    /// </summary>
    public DateTime? ReceiptDate { get; set; }
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; }
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
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; } = 0;
    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; } = "";
}

/// <summary>
/// Export Report Item
/// </summary>
public class ExportReportItem : NormalReportItem
{
    /// <summary>
    /// Outcoming Qty
    /// </summary>
    public decimal OutcomingQty { get; set; } = 0;
    /// <summary>
    /// CustomerId
    /// </summary>
    public int? CustomerId { get; set; }
    /// <summary>
    /// Customer Name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>
    /// Export Date
    /// </summary>
    public DateTime? ExportDate { get; set; }
}

/// <summary>
/// Import Report Item
/// </summary>
public class ImportReportItem : NormalReportItem
{
    /// <summary>
    /// Incoming Qty
    /// </summary>
    public decimal IncomingQty { get; set; } = 0;
    /// <summary>
    /// Supplier Id
    /// </summary>
    public int? SupplierId { get; set; }
    /// <summary>
    /// Supplier Name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;
    /// <summary>
    /// Import Date
    /// </summary>
    public DateTime? ImportDate { get; set; }
}