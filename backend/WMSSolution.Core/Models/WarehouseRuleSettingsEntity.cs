
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Warehouse;

/// <summary>
/// RuleSettings
/// </summary>
[Table("warehouserulesettings")]
public class WarehouseRuleSettingsEntity : BaseModel
{
    /// <summary>
    /// SupplierId
    /// </summary>
    [Column("supplier_id")]
    public int? SupplierId { get; set; }
    /// <summary>
    /// BlockId
    /// </summary>
    [Column("block_id")]
    public int? BlockId { get; set; }
    /// <summary>
    /// FloorId
    /// </summary>
    [Column("floor_id")]
    public int? FloorId { get; set; }
    /// <summary>
    /// SkuId
    /// </summary>
    [Column("sku_id")]
    public int? SkuId { get; set; }
    /// <summary>
    /// CategoryId
    /// </summary>
    [Column("category_id")]
    public int? CategoryId { get; set; }
    /// <summary>
    /// RuleSettings
    /// </summary>
    [Column("rule_settings")]
    public string RuleSettings { get; set; } = "FEFO";
    /// <summary>
    /// tenant_id
    /// </summary>
    public long tenant_id { get; set; } = 0;
}