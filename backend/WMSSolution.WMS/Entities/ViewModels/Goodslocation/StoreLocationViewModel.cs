namespace WMSSolution.WMS.Entities.ViewModels.Goodslocation;

/// <summary>
/// Store Location ViewModel
/// </summary>
public class StoreLocationViewModel : LocationDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// PalletCode
    /// </summary>
    public string PalletCode { get; set; } = "";
    /// <summary>
    /// PalletName
    /// </summary>
    public string PalletName { get; set; } = "";
    /// <summary>
    /// Products
    /// </summary>
    public List<ProductDto> Products { get; set; } = [];
    /// <summary>
    /// Virtual Location
    /// </summary>
    public bool VirtualLocation { get; set; } = false;
}

/// <summary>
/// Location Dto
/// </summary>
public class LocationDto
{
    /// <summary>
    /// Address
    /// </summary>
    public string Address { get; set; } = "";
    /// <summary>
    /// Level
    /// </summary>
    public int? Level { get; set; } = 1;
    /// <summary>
    /// Type
    /// </summary>
    public string Type { get; set; } = "";
    /// <summary>
    /// Status
    /// </summary>
    public int? Status { get; set; } = 1;
    /// <summary>
    /// CoordX
    /// </summary>
    public int? CoordX { get; set; }
    /// <summary>
    /// CoordY
    /// </summary>
    public int? CoordY { get; set; }
    /// <summary>
    /// CoordZ
    /// </summary>
    public int? CoordZ { get; set; }
    /// <summary>
    /// StoragePriority
    /// </summary>
    public int? StoragePriority { get; set; } = 1;
}

/// <summary>
/// Product Dto
/// </summary>
public class ProductDto
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; }
    /// <summary>
    /// SkuCode
    /// </summary>
    public string SkuCode { get; set; } = "";
    /// <summary>
    /// SkuName
    /// </summary>
    public string SkuName { get; set; } = "";
    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Available Quantity
    /// </summary>
    public decimal? AvailableQty { get; set; }

    /// <summary>
    /// Expiry Date
    /// </summary>
    public DateTime ExpiryDate { get; set; }
}