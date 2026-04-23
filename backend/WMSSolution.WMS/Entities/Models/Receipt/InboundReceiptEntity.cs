using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Shared Receipt Entity
/// </summary>
[Table("shared_receipts")]
public class SharedReceiptEntity : BaseModel, ITenantEntity
{
    /// <summary>
    /// Tennant Id
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 1;
    /// <summary>
    /// Creator
    /// </summary>
    [Column("creator")]
    public string? Creator { get; set; } = string.Empty;
    /// <summary>
    /// Create Date
    /// </summary>
    [Column("create_date")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Receipt number
    /// </summary>
    [Column("receipt_number")]
    public string? ReceiptNumber { get; set; }
    /// <summary>
    /// Receipt Id
    /// </summary>
    [Column("receipt_id")]
    public int? ReceiptId { get; set; }
    /// <summary>
    /// InboundReceipt
    /// </summary>
    [Column("inbound_receipt")]
    public bool InboundReceipt { get; set; } = true;
    /// <summary>
    /// Share Key
    /// </summary>
    [Column("share_key")]
    public string ShareKey { get; set; } = "";
}

/// <summary>
/// Inbound receipt entity
/// for Warehouse receipt order
/// </summary>
[Table("inbound_receipt")]
public class InboundReceiptEntity : BaseReceiptEntity
{
    /// <summary>
    /// Supplier Id
    /// </summary>
    [Column("supplier_id")]
    public required int SupplierId { get; set; }

    /// <summary>
    /// Is Stored
    /// </summary>
    [Column("is_stored")]
    public bool? IsStored { get; set; } = true;

    /// <summary>
    /// Expected Delivery Date
    /// </summary>
    [Column("expected_delivery_date")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Detail of receipt
    /// </summary>
    public virtual ICollection<InboundReceiptDetailEntity> Details { get; set; } = [];

    /// <summary>
    /// Pallet Details
    /// </summary>
    public virtual ICollection<InboundPalletEntity> PalletDetails { get; set; } = [];

    /// <summary>
    /// Multi Pallets
    /// </summary>
    [Column("multi_pallets")]
    public bool? MultiPallets { get; set; } = false;
}
