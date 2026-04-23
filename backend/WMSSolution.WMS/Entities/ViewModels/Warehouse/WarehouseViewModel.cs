
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WMSSolution.WMS.Entities.ViewModels.Warehouse;

/// <summary>
/// warehouse viewModel
/// </summary>
public class WarehouseViewModel
{

    #region constructor
    /// <summary>
    /// constructor
    /// </summary>
    public WarehouseViewModel()
    {

    }
    #endregion
    #region Property

    /// <summary>
    /// id
    /// </summary>
    [Display(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    /// <summary>
    /// WarehouseName
    /// </summary>
    [Display(Name = "WarehouseName")]
    [MaxLength(32, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// city
    /// </summary>
    [Display(Name = "city")]
    [MaxLength(128, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string city { get; set; } = string.Empty;    

    /// <summary>
    /// address
    /// </summary>
    [Display(Name = "address")]
    [MaxLength(256, ErrorMessage = "MaxLength")]
    [Required(ErrorMessage = "Required")]
    public string address { get; set; } = string.Empty;

    /// <summary>
    /// email
    /// </summary>
    [Display(Name = "email")]
    [MaxLength(128, ErrorMessage = "MaxLength")]
    public string email { get; set; } = string.Empty;

    /// <summary>
    /// manager
    /// </summary>
    [Display(Name = "manager")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string manager { get; set; } = string.Empty;

    /// <summary>
    /// contact_tel
    /// </summary>
    [Display(Name = "contact_tel")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string contact_tel { get; set; } = string.Empty;

    /// <summary>
    /// creator
    /// </summary>
    [Display(Name = "creator")]
    [MaxLength(64, ErrorMessage = "MaxLength")]
    public string creator { get; set; } = string.Empty;

    /// <summary>
    /// create_time
    /// </summary>
    [Display(Name = "create_time")]
    [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
    public DateTime create_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// last_update_time
    /// </summary>
    [Display(Name = "last_update_time")]
    [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
    public DateTime last_update_time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// is_valid
    /// </summary>
    [Display(Name = "is_valid")]
    public bool is_valid { get; set; } = true;

    /// <summary>
    /// tenant_id
    /// </summary>
    [Display(Name = "tenant_id")]
    public long tenant_id { get; set; } = 0;


    #endregion

    /// <summary>
    /// LocationCount
    /// </summary>
    public int LocationCount { get; set; }
    /// <summary>
    /// Total Pallets
    /// </summary>
    public int TotalPallet { get; set; }
    /// <summary>
    /// TotalInventory
    /// </summary>
    public int TotalInventory { get; set; }
    /// <summary>
    /// Wcs Block Id
    /// </summary>
    public string? WcsBlockId { get; set; }

}

/// <summary>
/// Warehouse settings
/// </summary>
public class WarehouseSettingsViewModel : StoreRuleSetting
{
    /// <summary>
    /// warehouse id
    /// </summary>
    public int WarehouseId { get; set; } = 0;

}
/// <summary>
/// Store Rule Setting
/// </summary>
public class StoreRuleSetting
{
    /// <summary>
    /// SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
    /// <summary>
    /// BlockId
    /// </summary>
    public int? BlockId { get; set; }
    /// <summary>
    /// FloorId
    /// </summary>
    public int? FloorId { get; set; }
    /// <summary>
    /// SkuId
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// CategoryId
    /// </summary>
    public int? CategoryId { get; set; }
    /// <summary>
    /// RuleSettings
    /// </summary>
    public string RuleSettings { get; set; } = "FEFO";
}

/// <summary>
/// Rule Settings
/// </summary>
public class RuleSettingsViewModel : StoreRuleSetting
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Supplier Name
    /// </summary>
    public string? SupplierName { get; set; } = "";
    /// <summary>
    /// Block Name
    /// </summary>
    public string? BlockName { get; set; } = "";
    /// <summary>
    /// Floor Name
    /// </summary>
    public string? FloorName { get; set; } = "";
    /// <summary>
    /// Sku Name
    /// </summary>
    public string? SkuName { get; set; } = "";
    /// <summary>
    /// Category Name
    /// </summary>
    public string? CategoryName { get; set; } = "";
}

/// <summary>
/// General Info
/// </summary>
public class WarehouseGeneralInfo
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; } = "";
    /// <summary>
    /// Address
    /// </summary>
    public string Address { get; set; } = "";
    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = "";
    /// <summary>
    /// LocationCount
    /// </summary>
    public int LocationCount { get; set; }
    /// <summary>
    /// Gets or sets the total number of items available in the inventory.
    /// </summary>
    /// <remarks>This property represents the cumulative count of all items currently tracked in the
    /// inventory. Ensure that this value is kept up to date to accurately reflect inventory changes resulting from
    /// additions, removals, or adjustments.</remarks>
    public int TotalInventory { get; set; }
    /// <summary>
    /// TotalPallet
    /// </summary>
    public int TotalPallet { get; set; }
    /// <summary>
    /// Processing Orders
    /// </summary>
    public int ProcessingOrders { get; set; }
    /// <summary>
    /// WcsBlockId
    /// </summary>
    public string? WcsBlockId { get; set; }

}

///// <summary>
///// Warehouse Rule Info
///// </summary>
//public class WarehouseRuleInfo
//{
//    /// <summary>
//    /// Rule Settings
//    /// </summary>
//    public IEnumerable<StoreRuleSettingsDto> RuleSettings { get; set; } = [];
//    /// <summary>
//    /// Suppliers
//    /// </summary>
//    public IEnumerable<SupplierDTO> Suppliers { get; set; } = [];
//    /// <summary>
//    /// Gets or sets the collection of floor numbers in the building.
//    /// </summary>
//    /// <remarks>The collection represents the individual floors and can be used to determine the layout of
//    /// the building. Each floor is represented by an integer value.</remarks>
//    public IEnumerable<int> Floors { get; set; } = [];
//    /// <summary>
//    /// Skus
//    /// </summary>
//    public IEnumerable<SkuSelectDTO> Skus { get; set; } = [];
//    /// <summary>
//    /// Store Locations
//    /// </summary>
//    public IEnumerable<StoreLocationDto> StoreLocations { get; set; } = [];
//    /// <summary>
//    /// Virtual Locations
//    /// </summary>
//    public IEnumerable<StoreLocationDto> VirtualLocations { get; set; } = [];
//}

///// <summary>
///// Store Location
///// </summary>
//public class StoreLocationDto
//{
//    /// <summary>
//    /// Id
//    /// </summary>
//    public int Id { get; set; }
//    /// <summary>
//    /// Name
//    /// </summary>
//    public string Name { get; set; } = "";
//}

///// <summary>
///// Sku Select
///// </summary>
//public class SkuSelectDTO
//{
//    /// <summary>
//    /// sku_id
//    /// </summary>
//    public int sku_id { get; set; }
//    /// <summary>
//    /// Sku Code
//    /// </summary>
//    public string SkuCode { get; set; } = "";
//    /// <summary>
//    /// Sku Name
//    /// </summary>
//    public string SkuName { get; set; } = "";
//}

///// <summary>
///// Supplier
///// </summary>
//public class SupplierDTO
//{
//    /// <summary>
//    /// Id
//    /// </summary>
//    public int Id { get; set; }
//    /// <summary>
//    /// Supplier Name
//    /// </summary>
//    public string SupplierName { get; set; } = "";
//    /// <summary>
//    /// City
//    /// </summary>
//    public string City { get; set; } = "";
//    /// <summary>
//    /// Address
//    /// </summary>
//    public string Address { get; set; } = "";
//    /// <summary>
//    /// Email
//    /// </summary>
//    public string Email { get; set; } = "";
//    /// <summary>
//    /// Gets or sets the name of the manager responsible for overseeing this entity.
//    /// </summary>
//    public string Manager { get; set; } = "";
//    /// <summary>
//    /// Contact Tel
//    /// </summary>
//    public string ContactTel { get; set; } = "";
//}

///// <summary>
///// Store RuleSettings
///// </summary>
//public class StoreRuleSettingsDto
//{
//    /// <summary>
//    /// Id
//    /// </summary>
//    public int Id { get; set; }
//    /// <summary>
//    /// Warehouse Id
//    /// </summary>
//    public int WarehouseId { get; set; }
//    public int? SupplierId { get; set; }
//    public int? BlockId { get; set; }
//    public int? FloorId { get; set; }
//    public int? SkuId { get; set; }
//    public int? CategoryId { get; set; }
//    public string RuleSettings { get; set; } = "FEFO";
//}