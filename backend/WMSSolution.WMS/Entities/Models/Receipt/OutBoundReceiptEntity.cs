using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Outbound Receipt entity
/// </summary>
[Table("outbound_receipt")]
public class OutBoundReceiptEntity : BaseReceiptEntity
{
    /// <summary>
    /// Customer ref
    /// </summary>
    [Column("customer_id")]
    public required int CustomerId { get; set; }
    
    /// <summary>
    /// Outbound Gateway
    /// </summary>
    [Column("outbound_gateway_id")]
    public required int OutboundGatewayId { get; set; }

    /// <summary>
    /// start picking time
    /// </summary>
    [Column("start_picking_time")]
    public DateTime? StartPickingTime { get; set; }

    /// <summary>
    /// Expected ship date
    /// </summary>
    [Column("expected_ship_date")]
    public DateTime? ExpectedShipDate { get; set; }

    /// <summary>
    /// Detail of receipt
    /// </summary>
    public virtual ICollection<OutBoundReceiptDetailEntity> Details { get; set; } = [];
    /// <summary>
    /// Receipt Date
    /// </summary>
    [Column("receipt_date")]
    public DateTime? ReceiptDate { get; set; }
}
