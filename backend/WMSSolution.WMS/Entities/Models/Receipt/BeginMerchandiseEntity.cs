using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Receipt;

/// <summary>
/// Begin Merchandises
/// </summary>
[Table("BeginMerchandises")]
public class BeginMerchandiseEntity : BaseModel, ITenantEntity
{
    /// <summary>
    /// Tennant Id
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 1;
    /// <summary>
    /// WareHouseId
    /// </summary>
    public int? WarehouseId { get; set; }
    /// <summary>
    /// SkuId
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// Quantity
    /// </summary>
    public decimal? Quantity { get; set; }
    /// <summary>
    /// SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
    /// <summary>
    /// ExpireDate
    /// </summary>
    public DateTime? ExpireDate { get; set; }
    /// <summary>
    /// Uom Id
    /// </summary>
    public int? UomId { get; set; }
    /// <summary>
    /// LocationId
    /// </summary>
    public int? LocationId { get; set; } 
    /// <summary>
    /// Created Date
    /// </summary>
    public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Creator
    /// </summary>
    public string Creator { get; set; } = string.Empty;
    /// <summary>
    /// IsPutaway
    /// </summary>
    public bool? IsPutaway { get; set; } = false;
}
