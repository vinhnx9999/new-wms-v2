
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// action_log  entity 
    /// </summary>
    [Table("action_log")]
    public class ActionLogEntity : BaseModel, ITenantEntity
    {
        #region Property

        /// <summary>
        /// user_name
        /// </summary>
        public string user_name { get; set; } = string.Empty;

        /// <summary>
        /// action_content
        /// </summary>
        public string action_content { get; set; } = string.Empty;

        /// <summary>
        /// action_time
        /// </summary>
        public DateTime action_time { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// action_name
        /// </summary>
        public string action_name { get; set; } = string.Empty;
        /// <summary>
        /// tenant_id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;

        #endregion Property
    }
}