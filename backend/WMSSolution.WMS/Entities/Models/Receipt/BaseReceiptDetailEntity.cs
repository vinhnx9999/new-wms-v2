using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Base Receipt Detail Entity
/// </summary>
public abstract class BaseReceiptDetailEntity : BaseModel
{
    /// <summary>
    /// Sku Id
    /// </summary>
    [Column("sku_id")]
    public required int SkuId { get; set; }

    /// <summary>
    /// Quantity of items in the receipt
    /// </summary>
    [Column("quantity")]
    public required decimal Quantity { get; set; }

    /// <summary>
    /// unit of measure id  
    /// </summary>
    [Column("sku_uom_id")]
    public required int SkuUomId { get; set; }

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
    /// Price
    /// </summary>
    [Column("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// source number
    /// </summary>
    [Column("source_number")]
    public string? SourceNumber { get; set; }

}