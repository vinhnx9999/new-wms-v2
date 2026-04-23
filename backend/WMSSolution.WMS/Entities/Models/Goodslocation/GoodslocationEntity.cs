using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums.Location;

namespace WMSSolution.WMS.Entities.Models;

/// <summary>
/// goodslocation entity
/// </summary>
[Table("goodslocation")]
public class GoodslocationEntity : BaseModel, ITenantEntity
{
    #region Property

    /// <summary>
    /// WarehouseId
    /// </summary>
    [Column("warehouse_id")]
    [JsonPropertyName("warehouse_id")]
    public int WarehouseId { get; set; } = 0;

    /// <summary>
    /// WarehouseName
    /// </summary>
    [Column("warehouse_name")]
    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// warehouse_area_name
    /// format : Block A , Block B , Block C , etc
    /// </summary>
    [Column("warehouse_area_name")]
    [JsonPropertyName("warehouse_area_name")]
    public string WarehouseAreaName { get; set; } = string.Empty;

    /// <summary>
    /// WarehouseAreaProperty
    /// </summary>
    [Column("warehouse_area_property")]
    [JsonPropertyName("warehouse_area_property")]
    public byte WarehouseAreaProperty { get; set; } = 0;

    /// <summary>
    /// LocationName
    /// format : z.x.y
    /// </summary>
    [Column("location_name")]
    [JsonPropertyName("location_name")]
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// location_length
    /// </summary>
    [Column("location_length")]
    [JsonPropertyName("location_length")]
    public decimal LocationLength { get; set; } = 0;

    /// <summary>   
    /// location_width
    /// </summary>
    [Column("location_width")]
    [JsonPropertyName("location_width")]
    public decimal LocationWidth { get; set; } = 0;

    /// <summary>
    /// location_heigth
    /// </summary>
    [Column("location_heigth")]
    [JsonPropertyName("location_heigth")]
    public decimal LocationHeigth { get; set; } = 0;

    /// <summary>
    /// location_volume
    /// </summary>
    [Column("location_volume")]
    [JsonPropertyName("location_volume")]
    public decimal LocationVolume { get; set; } = 0;

    /// <summary>
    /// location_load
    /// Maximum load capacity of this location
    /// </summary>
    [Column("location_load")]
    [JsonPropertyName("location_load")]
    public decimal LocationLoad { get; set; } = 0;

    /// <summary>
    /// CoordinateX
    /// Coordinate X horizontal direction.
    /// x is horizontal of the warehouse area
    /// </summary>
    [Column("coordinate_X")]
    [JsonPropertyName("coordinate_X")]
    public string CoordinateX { get; set; } = string.Empty;

    /// <summary>
    /// CoordinateY
    /// Coordinate Y vertical direction
    /// Y is vertical of the warehouse area
    /// </summary>
    [Column("coordinate_Y")]
    [JsonPropertyName("coordinate_Y")]
    public string CoordinateY { get; set; } = string.Empty;

    /// <summary>
    /// CoordinateZ
    /// coordinate Z 
    /// Z is floor of the warehouse area
    /// </summary>
    [Column("coordinate_Z")]
    [JsonPropertyName("coordinate_Z")]
    public string CoordinateZ { get; set; } = string.Empty;

    /// <summary>
    /// LocationStatus
    /// the status of this location 
    /// 0 : available
    /// 1 : occupied
    /// 2 : locked
    /// </summary>
    [Column("location_status")]
    [JsonPropertyName("location_status")]
    public byte LocationStatus { get; set; } = 0;

    /// <summary>
    /// create_time
    /// </summary>
    [Column("create_time")]
    [JsonPropertyName("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    [Column("last_update_time")]
    [JsonPropertyName("last_update_time")]
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// is_valid
    /// bool check if this location has been check
    /// </summary>
    [Column("is_valid")]
    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; } = false;

    /// <summary>
    /// tenant_id
    /// </summary>
    [Column("tenant_id")]
    [JsonPropertyName("tenant_id")]
    public long TenantId { get; set; } = 0;

    /// <summary>
    /// warehouse_area_id
    /// </summary>
    [Column("warehouse_area_id")]
    [JsonPropertyName("warehouse_area_id")]
    public int WarehouseAreaId { get; set; } = 0;

    /// <summary>
    /// Column Priority
    /// 1 - 6   
    /// less number means higher priority for inbound
    /// bigger number means higher priority for outbound
    /// </summary>
    [Column("priority")]
    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 0;


    /// <summary>
    /// good location type  
    /// with 1 means Storage Slot
    /// with 2 means Horizontal Path
    /// </summary>
    [Column("goods_location_type")]
    [JsonPropertyName("goods_location_type")]
    public GoodsLocationTypeEnum GoodsLocationType { get; set; } = GoodsLocationTypeEnum.StorageSlot;
    #endregion

}
