namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

/// <summary>
/// Detailed DTO for a single inbound receipt with items
/// </summary>
public class InboundReceiptDetailedDto : BaseInboundReceiptDto
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
    /// Type of receipt (default: Inbound)
    /// </summary>
    public string Type { get; set; } = String.Empty;

    /// <summary>
    /// Create date 
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<InboundReceiptDetailItemDto> Details { get; set; } = [];

    /// <summary>
    /// Supplier Name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Status of the receipt
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Expected Arrival Date
    /// </summary>
    public DateTime? ExpectedArrivalDate { get; set; }
    /// <summary>
    /// Multi Pallets
    /// </summary>
    public bool? MultiPallets { get; set; } = false;
    /// <summary>
    /// SharingUrl
    /// </summary>
    public string SharingUrl { get; set; } = "";
}

/// <summary>
/// Detail item within a receipt
/// </summary>
public class InboundReceiptDetailItemDto : BaseReceiptDetailDto
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
    /// Expire Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Is Exception
    /// </summary>
    public bool IsException { get; set; } = false;
}
