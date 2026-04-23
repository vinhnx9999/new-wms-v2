namespace WMSSolution.WMS.Entities.ViewModels.Receipt;

/// <summary>
/// Begin Merchandise DTO for initial inventory data, used for data migration or inventory initialization
/// </summary>
public class BeginMerchandiseDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; } = 0;
    /// <summary>
    /// TenantId
    /// </summary>
    public long TenantId { get; set; } = 1;
    /// <summary>
    /// WareHouseId
    /// </summary>
    public int? WarehouseId { get; set; }
    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// SkuId
    /// </summary>
    public int? SkuId { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;
    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;
    /// <summary>
    /// Quantity
    /// </summary>
    public decimal? Quantity { get; set; }
    /// <summary>
    /// SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
    /// <summary>
    /// Supplier Name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;
    /// <summary>
    /// ExpireDate
    /// </summary>
    public DateTime? ExpireDate { get; set; }
    /// <summary>
    /// Uom Id
    /// </summary>
    public int? UomId { get; set; }
    /// <summary>
    /// Unit Name
    /// </summary>
    public string UnitName { get; set; } = string.Empty;
    /// <summary>
    /// LocationId
    /// </summary>
    public int? LocationId { get; set; }
    /// <summary>
    /// Location Name
    /// </summary>
    public string LocationName { get; set; } = string.Empty;
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
