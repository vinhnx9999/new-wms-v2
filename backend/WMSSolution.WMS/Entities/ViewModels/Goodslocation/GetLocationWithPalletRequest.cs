namespace WMSSolution.WMS.Entities.ViewModels.Goodslocation;

/// <summary>
/// Dto request for suggest location 
/// </summary>
public class GetLocationWithPalletRequest
{
    /// <summary>
    /// Warehouse Id
    /// </summary>
    public int WarehouseId { get; set; } = 0;

    /// <summary>
    /// Qty of items to be stored in the location
    /// </summary>
    public int Qty { get; set; } = 0;    
}

/// <summary>
/// 
/// </summary>
public class GetLocationWithSkuIdRequest : GetLocationWithPalletRequest
{
    /// <summary>
    /// SkuId
    /// </summary>
    public int? SkuId { get; set; }
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