using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Base Receipt Entity
/// </summary>
public class BaseReceiptEntity : BaseModel, ITenantEntity
{
    /// <summary>
    /// Receipt number
    /// </summary>
    [Column("receipt_number")]
    public required string ReceiptNumber { get; set; }

    /// <summary>
    /// type of this receipt
    /// </summary>
    [Column("type")]
    public string? Type { get; set; }

    /// <summary>
    /// get Warehouse info
    /// </summary>
    [Column("warehouse_id")]
    public required int WarehouseId { get; set; }

    /// <summary>
    /// Description of this Receipt
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

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
    /// Last update time
    /// </summary>
    [Column("last_update_time")]
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// reference to Outbound if have
    /// </summary>
    [Column("ref_receipt")]
    public int? RefReceipt { get; set; }

    /// <summary>
    /// Status of the receipt
    /// </summary>
    [Column("status")]
    public ReceiptStatus Status { get; set; }

    /// <summary>
    /// Tennant Id
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 1;

    /// <summary>
    /// A deliverer is a person or entity that transports goods, mail, or messages to a recipient, acting as a courier or delivery person
    /// </summary>
    [Column("deliverer")]
    public string? Deliverer { get; set; } = "";
    /// <summary>
    /// A consignee is the person, company, or entity designated to receive goods, cargo, or merchandise in a shipping or logistics transaction
    /// </summary>
    [Column("consignee")]
    public string? Consignee { get; set; } = "";

    /// <summary>
    /// Invoice number
    /// </summary>
    [Column("invoice_number")]
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Priority of the receipt, used for sorting and processing order. Higher values indicate higher priority.
    /// Default value with the min
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; } = 1;
}