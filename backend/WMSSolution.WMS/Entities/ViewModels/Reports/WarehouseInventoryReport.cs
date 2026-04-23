
namespace WMSSolution.WMS.Entities.ViewModels.Reports;

/// <summary>
/// Base Inventory Card Item
/// </summary>
public class BaseInventoryCardItem
{
    /// <summary>
    /// Detail Id
    /// </summary>
    public int? DetailId { get; set; }
    /// <summary>
    /// Trans date
    /// </summary>
    public DateTime TransactionDate { get; set; }
    /// <summary>
    /// Receipt Id
    /// </summary>
    public int? InReceiptId { get; set; }
    /// <summary>
    /// Out Receipt Id
    /// </summary>
    public int? OutReceiptId { get; set; }
    /// <summary>
    /// Inward Quantity
    /// </summary>
    public decimal InwardQuantity { get; set; } = 0;
    /// <summary>
    /// Outward Quantity
    /// </summary>
    public decimal OutwardQuantity { get; set; } = 0;
    /// <summary>
    /// SkuId
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// UnitId
    /// </summary>
    public int? UnitId { get; set; }
    /// <summary>
    /// LocationId
    /// </summary>
    public int? LocationId { get; set; }
}

/// <summary>
/// Inventory Card Item
/// </summary>
public class InventoryCardItem : InventoryReportItem
{
    /// <summary>
    /// Receipt Date
    /// </summary>
    public DateTime? ReceiptDate { get; set; }
    /// <summary>
    /// Receipt Id
    /// </summary>
    public int? InReceiptId { get; set; }

    /// <summary>
    /// ReceiptId
    /// </summary>
    public int ReceiptId
    {
        get
        {
            if (InReceiptId.HasValue) return InReceiptId.Value;
            return OutReceiptId.GetValueOrDefault();
        }
    }
    /// <summary>
    /// Out Receipt Id
    /// </summary>
    public int? OutReceiptId { get; set; }
    /// <summary>
    /// Ngày phát sinh nghiệp vụ
    /// </summary>
    public DateTime TransactionDate { get; set; }
    /// <summary>
    /// Warehouse Receipt - GRN: Chứng từ nhập
    /// </summary>
    public string GoodsReceiptNote { get; set; } = "";
    /// <summary>
    /// Delivery Note - GIN: Chứng từ xuất
    /// </summary>
    public string GoodsIssueNote { get; set; } = "";
    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = "";
    /// <summary>
    /// Closed Balance
    /// </summary>
    public decimal ClosedBalance;
}

/// <summary>
/// Warehouse Inventory Report
/// </summary>
public class WarehouseInventoryReport
{
    /// <summary>
    /// WarehouseId
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
    /// Items
    /// </summary>

    public List<InventoryReportItem> Items { get; set; } = [];
}
