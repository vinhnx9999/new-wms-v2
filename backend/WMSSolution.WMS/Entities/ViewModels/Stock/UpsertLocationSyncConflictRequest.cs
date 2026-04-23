namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// upsert location sync conflict request
    /// </summary>
    public class UpsertLocationSyncConflictRequest
    {
        /// <summary>
        /// warehouse id
        /// </summary>
        public int WarehouseId { get; set; }
        /// <summary>
        /// location id
        /// </summary>
        public int LocationId { get; set; } = 0;
        /// <summary>
        /// location name
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// Wcs status
        /// </summary>
        public byte WcsStatus { get; set; } // 0: not found in WCS, 1: found

        /// <summary>
        /// Wms has pallet or not
        /// </summary>
        public bool WmsHasPallet { get; set; }

        /// <summary>
        /// Reason conflict occurs
        /// </summary>
        public string Reason { get; set; } = string.Empty; // WCS_ONLY, WMS_ONLY

        /// <summary>
        /// Trace Id for logging and debugging purposes
        /// </summary>
        public string TraceId { get; set; } = default!;
    }
}
