namespace Wms.Theme.Web.Model.InboundReceipt;

public class RetryInboundRequest
{
    /// <summary>
    /// ReceiptId
    /// </summary>
    public int ReceiptId { get; set; }
    /// <summary>
    /// Details
    /// </summary>
    public List<RetryInboundDetailItem> Items { get; set; } = default!;
}

/// <summary>
/// Details item for retry inbound request
/// </summary>
public class RetryInboundDetailItem
{
    /// <summary>
    /// Detail id
    /// </summary>
    public int DetailId { get; set; }
    /// <summary>
    /// Location Id
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// Pallet Code
    /// </summary>
    public string PalletCode { get; set; } = default!;

}
