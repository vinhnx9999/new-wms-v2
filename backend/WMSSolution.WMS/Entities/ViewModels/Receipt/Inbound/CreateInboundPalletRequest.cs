namespace WMSSolution.WMS.Entities.ViewModels.Receipt.Inbound;

/// <summary>
/// Create inbound pallet request
/// </summary>
public class CreateInboundPalletRequest
{
    /// <summary>
    /// Pallet code 
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Pallet RFID
    /// </summary>
    public string? PalletRFID { get; set; }

    /// <summary>
    /// location id 
    /// </summary>
    public int LocationId { get; set; } = default!;

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Is mixed
    /// </summary>
    public bool IsMixed { get; set; } = false;

    /// <summary>
    /// Details of the pallet
    /// </summary>
    public List<CreateInboundPalletDetailRequest> Details { get; set; } = [];
}

/// <summary>
/// Create inbound pallet detail request
/// </summary>
public class CreateInboundPalletDetailRequest
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; }

    /// <summary>
    /// SkuUomId
    /// </summary>
    public int SkuUomId { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Supplier Id
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Expiry Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
}