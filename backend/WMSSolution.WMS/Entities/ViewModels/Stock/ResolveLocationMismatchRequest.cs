namespace WMSSolution.WMS.Entities.ViewModels.Stock;

/// <summary>
/// Requesst for handle this conflict
/// </summary>
public class ResolveLocationMismatchRequest
{
    /// <summary>
    /// warehouse id
    /// </summary>
    public int WarehouseId { get; set; }
    /// <summary>
    /// Pallet Code 
    /// </summary>
    public string PalletCode { get; set; } = default!;

    /// <summary>
    /// Wms Location
    /// </summary>
    public string WmsLocation { get; set; } = default!;
    /// <summary>
    /// WCS location
    /// </summary>
    public string WcsLocation { get; set; } = default!;
    /// <summary>
    /// Note
    /// </summary>
    public string? Note { get; set; }
}