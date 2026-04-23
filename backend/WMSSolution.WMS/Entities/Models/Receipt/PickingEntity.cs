using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Picking Entity
/// </summary>
[Table("planning_picking")]
public class PickingEntity : BaseModel, ITenantEntity
{
    /// <summary>
    /// Receipt detail Id
    /// </summary>
    [Column("picking_id")]
    public int? PickingId { get; set; } = 0;
    /// <summary>
    /// ReceiptId
    /// </summary>
    [Column("receipt_id")]
    public int? ReceiptId { get; set; } = 0;
    /// <summary>
    /// Warehouse ID
    /// </summary>
    [Column("warehouse_id")]
    public int? WarehouseId { get; set; }
    /// <summary>
    /// Sku Id
    /// </summary>
    [Column("sku_id")]
    public int SkuId { get; set; }

    /// <summary>
    /// Quantity of items in the receipt
    /// </summary>
    [Column("quantity")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// unit of measure id  
    /// </summary>
    [Column("sku_uom_id")]
    public int SkuUomId { get; set; }

    /// <summary>
    /// Create date 
    /// </summary>
    [Column("create_date")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// LocationId
    /// </summary>
    [Column("goods_location_id")]
    public int? LocationId { get; set; }

    /// <summary>
    /// pallet code
    /// </summary>
    [Column("pallet_code")]
    public string? PalletCode { get; set; }
    /// <summary>
    /// Expire Date
    /// </summary>
    [Column("expiry_date")]
    public DateTime? ExpiryDate { get; set; }
    /// <summary>
    /// GatewayId
    /// </summary>
    [Column("gateway_id")]
    public int? GatewayId { get; set; }
    /// <summary>
    /// Tennant Id
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 1;
}
