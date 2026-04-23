namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;

/// <summary>
/// Update outbound receipt request
/// </summary>
public class UpdateOutboundReceiptRequest : BaseOutboundReceiptDto
{

    /// <summary>
    /// flag to change status draft -> new
    /// </summary>
    public bool IsUpgradeStatus { get; set; } = false;

    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<UpdateOutboundReceiptDetailDto> Details { get; set; } = [];
}

/// <summary>
/// Detail item for receipt creation
/// </summary>
public class UpdateOutboundReceiptDetailDto : BaseReceiptDetailDto
{
    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Outbound Order Ref
    /// </summary>
    public int? DispatchId { get; set; }
}

