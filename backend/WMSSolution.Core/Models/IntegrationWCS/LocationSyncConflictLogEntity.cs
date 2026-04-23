using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Shared.Enums;

namespace WMSSolution.Core.Models.IntegrationWCS
{
    /// <summary>
    /// Location sync conflict log entity represents a record of a conflict
    /// </summary>
    [Table("location_sync_conflict_log")]
    public class LocationSyncConflictLogEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// Trace id using for check for duplicate request,
        /// as the location sync process is triggered by WCS and we have seen duplicate requests from WCS,
        /// need to have this field to identify if the request is duplicate or not,
        /// and avoid creating duplicated conflict log entries.
        /// </summary>
        [Column("trace_id")]
        public string TraceId { get; set; } = default!;

        /// <summary>
        /// warehouse id
        /// </summary>
        [Column("warehouse_id")]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Source system
        /// </summary>
        [Column("source_system")]
        public string? SourceSystem { get; set; }

        /// <summary>
        /// Action time in wcs doing this location sync
        /// </summary>
        [Column("action_time")]
        public DateTime? ActionTime { get; set; } = default!;

        /// <summary>
        /// Received time of the conflict log entry
        /// </summary>
        [Column("received_time")]
        public DateTime ReceivedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Completed time of the conflict resolution, null if not resolved yet
        /// </summary>
        [Column("completed_time")]
        public DateTime? CompletedTime { get; set; }

        /// <summary>
        /// Number of conflicts inserted
        /// </summary>
        [Column("conflict_inserted")]
        public int ConflictInserted { get; set; } = 0;

        /// <summary>
        /// Number of conflicts updated
        /// </summary>
        [Column("conflict_updated")]
        public int ConflictUpdated { get; set; } = 0;

        /// <summary>
        /// Tenant id 
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;

        /// <summary>
        /// Error message
        /// </summary>
        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [Column("status")]
        public ConflictStatus Status { get; set; } = ConflictStatus.Pending;
    }
}
