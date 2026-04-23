namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;

/// <summary>
/// Inbound receipt list DTO .
/// </summary>
public class OutboundReceiptListResponse: BaseOutboundReceiptDto
{
    /// <summary>
    ///  id 
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// warehouse name 
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string WarehouseAddress { get; set; } = string.Empty;
    /// <summary>
    /// customer name 
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>
    /// creator
    /// </summary>
    public string Creator { get; set; } = string.Empty;
    /// <summary>
    /// create date
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Outbound gateway name
    /// </summary>
    public string OutBoundGatewayName { get; set; } = string.Empty;

    /// <summary>
    /// Status of Outbound Receipt  
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Total Qty of Outbound Receipt
    /// </summary>
    public int TotalQty { get; set; } = 0;
    /// <summary>
    /// Last Updated Date
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }
}

