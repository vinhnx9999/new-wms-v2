namespace Wms.Theme.Web.Model.GoodLocation;

public class GetLocationWithPalletRequest
{
    /// <summary>
    /// Warehouse Id
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Qty of items to be stored in the location
    /// </summary>
    public int Qty { get; set; }
    
}

public class GetLocationWithSkuIdRequest : GetLocationWithPalletRequest
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int SkuId { get; set; } = 0;
    /// <summary>
    /// SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
    /// <summary>
    /// Quantity requested by the customer
    /// </summary>
    public int? RequestedQuantity { get; set; }
    /// <summary>
    /// Rule settings
    /// </summary>
    public bool? ApplyRuleSettings { get; set; }
}