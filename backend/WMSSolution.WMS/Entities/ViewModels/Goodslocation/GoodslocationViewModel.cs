
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels;

/// <summary>
/// goodslocation viewModel
/// </summary>
public class GoodslocationViewModel
{

    #region constructor
    /// <summary>
    /// constructor
    /// </summary>
    public GoodslocationViewModel()
    {

    }
    #endregion
    #region Property


    /// <summary>
    /// id
    /// </summary>
    [Display(Name = "id")]
    public int Id { get; set; } = 0;

    /// <summary>
    /// WarehouseId
    /// </summary>
    [Display(Name = "WarehouseId")]
    public int WarehouseId { get; set; } = 0;

    /// <summary>
    /// WarehouseName
    /// </summary>
    [Display(Name = "WarehouseName")]
    [Required(ErrorMessage = "Required")]
    [MaxLength(32, ErrorMessage = "MaxLength")]
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// warehouse_area_name
    /// </summary>
    [Display(Name = "warehouse_area_name")]
    [MaxLength(32, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string WarehouseAreaName { get; set; } = string.Empty;

    /// <summary>
    /// WarehouseAreaProperty
    /// </summary>
    [Display(Name = "WarehouseAreaProperty")]
    [Required(ErrorMessage = "Required")]
    public byte WarehouseAreaProperty { get; set; } = 0;

    /// <summary>
    /// LocationName
    /// </summary>
    [Display(Name = "LocationName")]
    [Required(ErrorMessage = "Required")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// location_length
    /// </summary>
    [Display(Name = "location_length")]
    public decimal LocationLength { get; set; } = 0;

    /// <summary>
    /// location_width
    /// </summary>
    [Display(Name = "location_width")]
    public decimal LocationWidth { get; set; } = 0;

    /// <summary>
    /// location_heigth
    /// </summary>
    [Display(Name = "location_heigth")]
    public decimal LocationHeigth { get; set; } = 0;

    /// <summary>
    /// location_volume
    /// </summary>
    [Display(Name = "location_volume")]
    public decimal LocationVolume { get; set; } = 0;

    /// <summary>
    /// location_load
    /// </summary>
    [Display(Name = "location_load")]
    public decimal LocationLoad { get; set; } = 0;

    /// <summary>
    /// CoordinateX
    /// </summary>
    [Display(Name = "CoordinateX")]
    [MaxLength(10, ErrorMessage = "MaxLength")]
    public string CoordinateX { get; set; } = string.Empty;

    /// <summary>
    /// CoordinateY
    /// </summary>
    [Display(Name = "CoordinateY")]
    [MaxLength(10, ErrorMessage = "MaxLength")]
    public string CoordinateY { get; set; } = string.Empty;

    /// <summary>
    /// CoordinateZ
    /// </summary>
    [Display(Name = "CoordinateZ")]
    [MaxLength(10, ErrorMessage = "MaxLength")]
    public string CoordinateZ { get; set; } = string.Empty;

    /// <summary>
    /// LocationStatus
    /// </summary>
    [Display(Name = "LocationStatus")]
    [MaxLength(10, ErrorMessage = "MaxLength")]
    public byte LocationStatus { get; set; } = 0;

    /// <summary>
    /// create_time
    /// </summary>
    [Display(Name = "create_time")]
    [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    [Display(Name = "last_update_time")]
    [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// is_valid
    /// </summary>
    [Display(Name = "is_valid")]
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// tenant_id
    /// </summary>
    [Display(Name = "tenant_id")]
    public long TenantId { get; set; } = 0;

    /// <summary>
    /// warehouse_area_id
    /// </summary>
    [Display(Name = "warehouse_area_id")]
    public int WarehouseAreaId { get; set; } = 0;


    #endregion

}
