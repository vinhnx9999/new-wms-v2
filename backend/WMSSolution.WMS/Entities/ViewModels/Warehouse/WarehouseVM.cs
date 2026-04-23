using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels.Warehouse;

/// <summary>
/// Warehouse ViewModel
/// </summary>
public class WarehouseVM
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; } = 0;

    /// <summary>
    /// WarehouseName
    /// </summary>
    [MaxLength(32, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// city
    /// </summary>
    [MaxLength(128, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// address
    /// </summary>
    [MaxLength(256, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// email
    /// </summary>
    [MaxLength(128, ErrorMessage = "MaxLength")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// manager
    /// </summary>
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string Manager { get; set; } = string.Empty;

    /// <summary>
    /// contact_tel
    /// </summary>
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string ContactTel { get; set; } = string.Empty;

    /// <summary>
    /// creator
    /// </summary>
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string Creator { get; set; } = string.Empty;

    /// <summary>
    /// create_time
    /// </summary>
    [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// is_valid
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// tenant_id
    /// </summary>
    public long TenantId { get; set; } = 0;

}
