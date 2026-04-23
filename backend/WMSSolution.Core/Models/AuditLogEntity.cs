using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.ActionLog
{
    /// <summary>
    /// Audit log table entity
    /// </summary>
    [Table("audit_log")]
    public class AuditLogEntity : BaseModel, ITenantEntity
    {
        /// <summary>
        /// Table name
        /// </summary>
        [Column("table_name")]
        public required string TableName { get; set; }

        /// <summary>
        /// target id of record
        /// </summary>
        [Column("record_id")]
        public required string RecordId { get; set; }

        /// <summary>
        /// Action performed (INSERT, UPDATE, DELETE)
        /// </summary>
        [Column("action")]
        public required string Action { get; set; }

        /// <summary>
        /// old values
        /// </summary>
        [Column("old_values", TypeName = "jsonb")]
        public string? OldValues { get; set; }

        /// <summary>
        /// new values
        /// </summary>
        [Column("new_values", TypeName = "jsonb")]
        public string? NewValues { get; set; }

        /// <summary>
        /// action time
        /// </summary>
        [Column("action_time")]
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tennat id 
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; }
    }
}
