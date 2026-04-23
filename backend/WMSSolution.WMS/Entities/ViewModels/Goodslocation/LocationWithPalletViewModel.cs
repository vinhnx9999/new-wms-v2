namespace WMSSolution.WMS.Entities.ViewModels.Goodslocation;

/// <summary>
/// Good Location Info
/// </summary>
public class GoodLocationInfo
{
    /// <summary>
    /// location Id
    /// </summary>
    public int GoodLocationId { get; set; } = 0;
    /// <summary>
    /// location Name
    /// </summary>
    public string GoodLocationName { get; set; } = string.Empty;
    /// <summary>
    /// pallet Id
    /// </summary>
    public int PalletId { get; set; } = 0;
    /// <summary>
    /// pallet Code
    /// </summary>
    public string PalletCode { get; set; } = string.Empty;
    /// <summary>
    /// Available Quantity in pallet, it will be used to compare with the requested quantity to determine if the location is suitable for putaway
    /// </summary>
    public int AvailableQuantity { get; set; } = 1;
}

/// <summary>
/// Good Sku Location Info
/// </summary>
public class GoodSkuLocationInfo : GoodLocationInfo
{
    /// <summary>
    /// SKU Id, it will be used to compare with the requested SKU Id to determine if the location is suitable for putaway
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;
    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;
    /// <summary>
    /// Expiry Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    /// <summary>
    /// Supplier Id
    /// </summary>
    public int? SupplierId { get; set; } = 0;
    /// <summary>
    /// Supplier Name
    /// </summary>
    public string? SupplierName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int CurrentQuantity { get;  set; } = 0;
    /// <summary>
    /// VirtualLocation
    /// </summary>
    public bool VirtualLocation { get; set; }
}

/// <summary>
/// View Model Location With Pallet
/// </summary>
public class LocationWithPalletViewModel: GoodLocationInfo
{    
    /// <summary>
    /// item in pallet
    /// </summary>
    public List<ItemInPalletViewModel>? Items { get; set; }
}

/// <summary>
/// Item in pallet
/// </summary>
public class ItemInPalletViewModel
{
    /// <summary>
    /// sku id 
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// sku name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// Qty in each sku in pallet
    /// </summary>
    public int Qty { get; set; } = 0;
}
