namespace Wms.Theme.Web.Model.OutboundReceipt;

public class CreateOutboundReceiptRequest: BaseOutboundReceiptDto
{
    public bool IsDraft { get; set; } = true;
    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<CreateOutboundReceiptDetailDto> Details { get; set; } = [];
}

/// <summary>
/// Detail item for receipt creation
/// </summary>
public class CreateOutboundReceiptDetailDto: BaseOutboundDetailDto
{
    /// <summary>
    /// Outbound Order Ref
    /// </summary>
    public int? DispatchId { get; set; }
}
