namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Outbound;

/// <summary>
/// Outbound detailed DTO
/// </summary>
public class OutboundReceiptDetailedDto: BaseOutboundReceiptDto
{
    /// <summary>
    /// id of the receipt
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Warehouse Name
    /// </summary>  
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string WarehouseAddress { get; set; } = string.Empty;
    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<OutboundReceiptDetailItemDto> Details { get; set; } = [];

    /// <summary>
    /// Customer Name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Outbound gateway name
    /// </summary>
    public string OutboundGatewayName { get; set; } = default!;

    /// <summary>
    /// Status of the receipt
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// Sharing Url
    /// </summary>
    public string SharingUrl { get; set; } = string.Empty;
}

/// <summary>
/// Detail item within a receipt
/// </summary>
public class OutboundReceiptDetailItemDto: BaseReceiptDetailDto
{
    
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// Unit name
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// LocationName 
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Is Exception
    /// </summary>
    public bool IsException { get; set; } = false;
}

