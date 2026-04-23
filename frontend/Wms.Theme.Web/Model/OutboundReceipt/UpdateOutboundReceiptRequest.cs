namespace Wms.Theme.Web.Model.OutboundReceipt;
public class UpdateOutboundReceiptRequest: BaseOutboundReceiptDto
{
    /// <summary>
    /// Create date `
    /// </summary>
    public DateTime? CreatedDate { get; set; }

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
public class UpdateOutboundReceiptDetailDto: BaseOutboundDetailDto
{
    /// <summary>
    /// Id of detail item
    /// </summary>
    public int Id { get; set; } = 0;

    /// <summary>
    /// Outbound Order Ref
    /// </summary>
    public int? DispatchId { get; set; }
}
