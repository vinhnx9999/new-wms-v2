namespace Wms.Theme.Web.Model.Stock;

public class ResolveLocationMismatchRequest
{
    /// <summary>
    /// warehouse id
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// pallet code
    /// </summary>
    public string PalletCode { get; set; } = default!;
    /// <summary>
    /// wms location
    /// </summary>
    public string WmsLocation { get; set; } = default!;

    /// <summary>
    /// wcs location
    /// </summary>
    public string WcsLocation { get; set; } = default!;

    /// <summary>
    /// Note
    /// </summary>
    public string? Note { get; set; }
}