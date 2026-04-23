using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Inbound Pallet Entity
/// </summary>
[Table("inbound_pallets")]
public class InboundPalletEntity : BaseReceiptDetailEntity, ITenantEntity
{
    /// <summary>
    ///  Receipt Id
    /// </summary>
    [Column("receipt_id")]
    public int? ReceiptId { get; set; }

    /// <summary>
    /// Expected Delivery Date
    /// </summary>
    [Column("planning_date")]
    public DateTime? PlanningDate { get; set; }
    /// <summary>
    /// Receipt
    /// </summary>
    [ForeignKey(nameof(ReceiptId))]
    public virtual InboundReceiptEntity Receipt { get; set; } = null!;
    /// <summary>
    /// Tennant Id
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 1;
}

/// <summary>
/// Receipt details entity
/// </summary>
[Table("inbound_receipt_details")]
public class InboundReceiptDetailEntity : BaseReceiptDetailEntity
{
    /// <summary>
    ///  Receipt Id
    /// </summary>
    [Column("receipt_id")]
    public int ReceiptId { get; set; }
    /// <summary>
    /// Expire date
    /// </summary>
    [Column("expiry_date")]
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// FK to InboundReceipt
    /// </summary>
    [ForeignKey(nameof(ReceiptId))]
    public virtual InboundReceiptEntity Receipt { get; set; } = null!;

    /// <summary>
    /// Quantity of items in the receipt
    /// </summary>
    [Column("req_quantity")]
    public decimal? ReqQty { get; set; }

}
