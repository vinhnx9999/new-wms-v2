using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.Core.Models.IntegrationWCS
{
    /// <summary>
    /// LocationSyncConflictEntity represents a record of a conflict 
    /// that occurred during the synchronization of location data between the WMS and WCS systems. 
    /// This entity is used to log details about the conflict, such as the conflicting location information, timestamps, and any relevant identifiers.
    /// It serves as a basis for analyzing and resolving synchronization issues to ensure data consistency between the two systems.
    /// </summary>
    [Table("location_sync_conflict")]
    public class LocationSyncConflictEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// warehouse_id
        /// </summary>
        [Column("warehouse_id")]
        public int WarehouseId { get; set; }

        /// <summary>
        /// location id
        /// </summary>
        [Column("location_id")]
        public int LocationId { get; set; } = 0;

        /// <summary>
        /// location name
        /// </summary>
        [Column("location_name")]
        public string LocationName { get; set; } = default!;

        /// <summary>
        /// tenant id 
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;

        /// <summary>
        /// wcs status is sending 
        /// </summary>
        [Column("wcs_status")]
        public byte WcsStatus { get; set; }

        /// <summary>
        /// wms has pallet or not
        /// </summary>
        [Column("wms_has_pallet")]
        public bool WmsHasPallet { get; set; }

        /// <summary>
        /// Detailed description of the conflict reason
        /// </summary>
        [Column("reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// Status of the conflict 
        /// </summary>
        [Column("status")]
        public byte Status { get; set; }

        /// <summary>
        /// Creation timestamp of the conflict record,.
        /// </summary>
        [Column("created_time")]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID or Username of the person who resolved the conflict
        /// </summary>
        [Column("resolved_by")]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// resolve time of the conflict
        /// </summary>
        [Column("resolved_time")]
        public DateTime? ResolvedTime { get; set; }

        /// <summary>
        /// Resolve note 
        /// </summary>
        [Column("resolution_note")]
        public string? ResolutionNote { get; set; }

        /// <summary>
        /// TraceId for logging and debugging purposes
        /// </summary>
        [Column("trace_id")]
        public string TraceId { get; set; } = default!;
    }
}
