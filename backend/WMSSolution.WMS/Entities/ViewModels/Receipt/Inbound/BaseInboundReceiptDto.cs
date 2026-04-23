namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

/// <summary>
/// Base Inbound Receipt
/// </summary>
public class BaseInboundReceiptDto : BaseReceiptDto
{
    /// <summary>
    /// Supplier Id
    /// </summary>
    public int SupplierId { get; set; }
    /// <summary>
    /// IsStrored
    /// </summary>
    public bool IsStored { get; set; } = true;

    /// <summary>
    /// Deliverer
    /// </summary>
    public string? Deliverer { get; set; }

    /// <summary>
    /// Invoice number
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Expected delivery date
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }
}
