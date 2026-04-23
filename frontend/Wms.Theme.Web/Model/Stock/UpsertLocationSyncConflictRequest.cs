namespace Wms.Theme.Web.Model.Stock;

public class UpsertLocationSyncConflictRequest
{
    public int WarehouseId { get; set; }
    public int LocationId { get; set; } = 0;
    public string LocationName { get; set; } = string.Empty;
    public byte WcsStatus { get; set; } // 0: not found in WCS, 1: found
    public bool WmsHasPallet { get; set; }
    public string Reason { get; set; } = string.Empty; // WCS_ONLY, WMS_ONLY, LOCATION_MISMATCH
}