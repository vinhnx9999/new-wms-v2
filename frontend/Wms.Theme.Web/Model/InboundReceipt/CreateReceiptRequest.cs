using WMSSolution.Shared.Planning;

namespace Wms.Theme.Web.Model.InboundReceipt;

public class CreateBulkReceiptRequest : CreateReceiptRequest
{
    public IEnumerable<AvailablePallet> StoredData { get; set; } = [];
}

/// <summary>
/// Request to create a new inbound receipt
/// </summary>
public class CreateReceiptRequest
{
    /// <summary>
    /// Receipt Number
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Type of receipt (default: Inbound)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Is Store in  Stock
    /// </summary>
    public bool IsStored { get; set; } = true;

    /// <summary>
    /// Supplier ID
    /// </summary>
    public int SupplierId { get; set; }
    /// <summary>
    /// Description of this Receipt
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Create date 
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    /// <summary>
    /// List of receipt details
    /// </summary>
    public List<CreateReceiptDetailDto> Details { get; set; } = [];

    /// <summary>
    /// Status of receipt, default is draft
    /// </summary>
    public bool IsDraft { get; set; } = true;

    /// <summary>
    /// Deliver 
    /// </summary>
    public string? Deliverer { get; set; }

    /// <summary>
    /// Invoice number
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Expected Delivery date
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Priority 
    /// </summary>
    public int Priority { get; set; } = default!;
}

/// <summary>
/// Detail item for receipt creation
/// </summary>
public class CreateReceiptDetailDto
{
    /// <summary>
    /// SKU ID
    /// </summary>
    public int SkuId { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of Measure ID
    /// </summary>
    public int SkuUomId { get; set; }

    /// <summary>
    /// Goods Location ID (Stock Location)
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Inbound Order Ref
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public int? AsnId { get; set; }

    /// <summary>
    ///  Source number
    /// </summary>
    public string? SourceNumber { get; set; }
}
