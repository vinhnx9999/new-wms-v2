namespace Wms.Theme.Web.Model.GoodLocation;

public class LocationWithPalletViewModel
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
    /// item in pallet
    /// </summary>
    public List<ItemInPalletViewModel>? Items { get; set; }
    public int AvailableQuantity { get; set; } = 1;

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
