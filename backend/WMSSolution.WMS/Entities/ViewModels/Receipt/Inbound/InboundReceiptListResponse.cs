namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

/// <summary>
/// Dto response for inbound receipt list
/// </summary>
public class InboundReceiptListResponse: BaseInboundReceiptDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Receipt Type
    /// </summary>
    public string ReceiptType { get; set; } = string.Empty;

    /// <summary>
    /// Supplier Name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>  
    public int Status { get; set; }

    /// <summary>
    /// Create date
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Total Qty Of This Inbound Receipt
    /// </summary>
    public int TotalQty { get; set; } = 0;

    /// <summary>
    /// Exception 
    /// Detail in task has been rejected
    /// </summary>
    public bool IsException { get; set; } = false;
    /// <summary>
    /// Last Updated Date
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }
    /// <summary>
    /// Multi-Pallets
    /// </summary>
    public bool? MultiPallets { get; set; } = false;
}
