namespace WMSSolution.WMS.Entities.ViewModels.Warehouse;

/// <summary>
/// Sync Wcs Location
/// </summary>
public class SyncWcsLocationViewModel
{
    /// <summary>
    /// WarehouseId
    /// </summary>
    public int WarehouseId { get; set; } = 0;

    /// <summary>
    /// WcsLocation
    /// </summary>
    public IEnumerable<WcsLocationDto> Locations { get; set; } = [];
    /// <summary>
    /// Wcs BlockId
    /// </summary>
    public string WcsBlockId { get; set; } = "";
}

/// <summary>
/// Wcs Location
/// </summary>
public class WcsLocationDto
{
    /// <summary>
    /// Address
    /// </summary>
    public string Address { get; set; } = "";
    /// <summary>
    /// Zone
    /// </summary>
    public string Zone { get; set; } = "";
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
    /// Storage Priority
    /// </summary>
    public int? StoragePriority { get; set; } = 1;
}

/// <summary>
/// Pallet Location
/// </summary>
public class PalletLocationSync : WcsLocationDto
{
    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }
}