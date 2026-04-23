namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

/// <summary>
/// Update Inbound Receipt
/// </summary>
public class UpdateInboundReceiptRequest: BaseInboundReceiptDto
{

    /// <summary>
    /// Type of receipt (default: Inbound)
    /// </summary>
    public string? Type { get; set; }
    /// <summary>
    /// Create date 
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<UpdateReceiptDetailDto> Details { get; set; } = [];

    /// <summary>
    /// flag to change status draft -> new
    /// </summary>
    public bool IsUpgradeStatus { get; set; } = false;
    /// <summary>
    /// Multi Pallets
    /// </summary>
    public bool MultiPallets { get; set; } = false;
}

/// <summary>
/// Detail item for receipt update
/// </summary>
public class UpdateReceiptDetailDto: BaseReceiptDetailDto
{
    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Expire Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Inbound Order Ref
    /// </summary>
    public int? AsnId { get; set; }
}
