using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Outbound receipt details entity
/// </summary>
[Table("outbound_receipt_details")]
public class OutBoundReceiptDetailEntity : BaseReceiptDetailEntity
{
    /// <summary>
    ///  Receipt Id
    /// </summary>
    [Column("receipt_id")]
    public int ReceiptId { get; set; }

    /// <summary>
    /// Ref dispatch Id for outbound Receipt
    /// </summary>
    [Column("dispatch_id")]
    public int? DispatchId { get; set; }

    /// <summary>
    /// FK to OutboundReceipt
    /// </summary>
    [ForeignKey(nameof(ReceiptId))]
    public virtual OutBoundReceiptEntity Receipt { get; set; } = null!;

    /// <summary>
    /// Quantity of items in the receipt
    /// </summary>
    [Column("req_quantity")]
    public decimal? ReqQty { get; set; }
}
