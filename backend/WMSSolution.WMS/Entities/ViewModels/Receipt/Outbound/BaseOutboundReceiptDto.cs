namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;

/// <summary>
/// Base Outbound DTO for both create and update operations
/// </summary>
public class BaseOutboundReceiptDto: BaseReceiptDto
{
    /// <summary>
    /// Outbound gateway ID
    /// </summary>
    public int OutboundGatewayId { get; set; }

    /// <summary>
    /// Customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Type of receipt (default: Outbound)
    /// </summary>
    public string? Type { get; set; } = OutboundTypeConstant.Outbound;

    /// <summary>
    /// Create date `
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    /// <summary>
    /// Người nhận hàng
    /// </summary>
    public string? Consignee { get; set; }
    /// <summary>
    /// Receipt Date
    /// </summary>
    public DateTime? ReceiptDate { get; set; }
    /// <summary>
    /// start picking time
    /// </summary>
    public DateTime? StartPickingTime { get; set; }

    /// <summary>
    /// Expected ship date
    /// </summary>
    public DateTime? ExpectedShipDate { get; set; }
}
