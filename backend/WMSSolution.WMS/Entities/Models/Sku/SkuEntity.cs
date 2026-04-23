
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Sku;

/// <summary>
/// sku  entity
/// </summary>
[Table("sku")]
public class SkuEntity : BaseModel, ITenantEntity
{
    #region navigational properties

    /// <summary>
    /// navigational properties
    /// </summary>
    [ForeignKey("spu_id")]
    public SpuEntity? Spu { get; set; }

    #endregion

    #region Property

    /// <summary>
    /// spu_id
    /// </summary>
    public int? spu_id { get; set; } = 0;

    /// <summary>
    /// sku_code
    /// </summary>
    public string sku_code { get; set; } = string.Empty;

    /// <summary>
    /// sku_name
    /// </summary>
    public string sku_name { get; set; } = string.Empty;

    /// <summary>
    /// bar_code
    /// will be revise in future
    /// </summary>
    public string? bar_code { get; set; } = string.Empty;

    /// <summary>
    /// weight
    /// </summary>
    public decimal? weight { get; set; } = 0;

    /// <summary>
    /// lenght
    /// </summary>
    public decimal? lenght { get; set; } = 0;

    /// <summary>
    /// width
    /// </summary>
    public decimal? width { get; set; } = 0;

    /// <summary>
    /// height
    /// </summary>
    public decimal? height { get; set; } = 0;

    /// <summary>
    /// volume
    /// </summary>
    public decimal? volume { get; set; } = 0;

    /// <summary>
    /// unit
    /// will be deleted in future
    /// </summary>
    public string? unit { get; set; } = string.Empty;

    /// <summary>
    /// cost
    /// </summary>
    public decimal? cost { get; set; } = 0;

    /// <summary>
    /// price
    /// </summary>
    public decimal? price { get; set; } = 0;

    /// <summary>
    /// create_time
    /// </summary>
    public DateTime create_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    public DateTime last_update_time { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// pallet_maximum_quantity
    /// </summary>
    public int? maxQtyPerPallet { get; set; } = 100;

    /// <summary>
    /// TennantId
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 1;

    /// <summary>
    /// is_tracking
    /// </summary>
    public bool is_tracking { get; set; } = false;


    #endregion

    #region Sku Safety Stock

    /// <summary>
    /// Sku Safety Stock
    /// </summary>
    public List<SkuSafetyStockEntity> detailList { get; set; } = [];

    #endregion
}