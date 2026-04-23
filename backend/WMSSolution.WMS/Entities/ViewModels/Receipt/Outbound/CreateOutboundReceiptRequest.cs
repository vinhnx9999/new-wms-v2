namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;

/// <summary>
/// Request to create a new inbound receipt
/// </summary>
public class CreateOutboundReceiptRequest: BaseOutboundReceiptDto
{    
    /// <summary>
    /// Is draft receipt    
    /// </summary>
    public bool IsDraft { get; set; } = true;

    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<CreateOutboundReceiptDetailDto> Details { get; set; } = [];
}



/// <summary>
/// Detail item for receipt creation
/// </summary>
public class CreateOutboundReceiptDetailDto: BaseReceiptDetailDto
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

/// <summary>
/// Constant values for receipt types
/// </summary>
public static class OutboundTypeConstant
{
    /// <summary>
    /// Outbound receipt type
    /// </summary>
    public const string Outbound = "Outbound";
}
