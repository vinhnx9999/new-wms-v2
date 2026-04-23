namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationSyncLogItemDto
    {
        /// <summary>
        /// trace id
        /// </summary>
        public string TraceId { get; set; } = default!;
        /// <summary>
        /// Action time 
        /// </summary>
        public DateTime? ActionTime { get; set; }

        /// <summary>
        /// Received time 
        /// </summary>
        public DateTime ReceivedTime { get; set; }

        /// <summary>
        /// Conflicr inserted records count
        /// </summary>
        public int ConflictInserted { get; set; }

        /// <summary>
        /// Conflict Updated
        /// </summary>
        public int ConflictUpdated { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; } = default!;
    }

    /// <summary>
    /// Location Sync Conflict Key Dto
    /// </summary>
    public class LocationSyncConflictKeyDto
    {
        /// <summary>
        /// trace Id 
        /// </summary>
        public string TraceId { get; set; } = default!;
        /// <summary>
        /// Location name 
        /// </summary>
        public string LocationName { get; set; } = default!;
        /// <summary>
        /// Reason
        /// </summary>
        public string Reason { get; set; } = default!;
        /// <summary>
        ///wms has pallet or not
        /// </summary>
        public bool WmsHasPallet { get; set; }
        /// <summary>
        /// WCS status
        /// </summary>
        public byte WcsStatus { get; set; }
    }
}
