
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Warehouse;

/// <summary>
/// warehouse  entity
/// </summary>
[Table("warehouse")]
public class WarehouseEntity : BaseModel, ITenantEntity
{
    #region Property

    /// <summary>
    /// WarehouseName
    /// </summary>
    [Column("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// city
    /// </summary>
    public string city { get; set; } = string.Empty;

    /// <summary>
    /// address
    /// </summary>
    public string address { get; set; } = string.Empty;

    /// <summary>
    /// email
    /// </summary>
    public string email { get; set; } = string.Empty;

    /// <summary>
    /// manager
    /// </summary>
    public string manager { get; set; } = string.Empty;

    /// <summary>
    /// contact_tel
    /// </summary>
    public string contact_tel { get; set; } = string.Empty;

    /// <summary>
    /// creator
    /// </summary>
    public string creator { get; set; } = string.Empty;

    /// <summary>
    /// create_time
    /// </summary>
    public DateTime create_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    public DateTime last_update_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// is_valid
    /// </summary>
    public bool is_valid { get; set; } = false;

    /// <summary>
    /// tenant_id
    /// </summary>
    [Column("tenant_id")]
    public long TenantId { get; set; } = 0;
    /// <summary>
    /// Wcs BlockId
    /// </summary>  
    public string? WcsBlockId { get; set; }

    #endregion

}
