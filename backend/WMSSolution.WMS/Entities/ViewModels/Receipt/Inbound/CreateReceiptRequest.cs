using WMSSolution.Shared.Planning;

namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

/// <summary>
/// Create Bulk Receipt Request
/// </summary>
public class CreateBulkReceiptRequest : CreateReceiptRequest
{
    /// <summary>
    /// Stored Data
    /// </summary>
    public IEnumerable<AvailablePallet> StoredData { get; set; } = [];
}

/// <summary>
/// Request to create a new inbound receipt
/// </summary>
public class CreateReceiptRequest : BaseInboundReceiptDto
{
    /// <summary>
    /// Type of receipt (default: Inbound)
    /// </summary>
    public string? Type { get; set; } = InboundTypeConstant.Inbound;

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
}

/// <summary>
/// Detail item for receipt creation
/// </summary>
public class CreateReceiptDetailDto : BaseReceiptDetailDto
{
    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Expire Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Inbound Order Ref
    /// </summary>
    public int? AsnId { get; set; }
}

/// <summary>
/// Constant values for receipt types
/// </summary>
public static class InboundTypeConstant
{
    /// <summary>
    /// Inbound receipt type
    /// </summary>
    public const string Inbound = "Inbound";
}
