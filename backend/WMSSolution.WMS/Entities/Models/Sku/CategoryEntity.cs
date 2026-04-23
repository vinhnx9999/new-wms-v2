
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// category  entity
    /// </summary>
    [Table("category")]
    public class CategoryEntity : BaseModel, ITenantEntity
    {

        #region Property

        /// <summary>
        /// category_name
        /// </summary>
        public string category_name { get; set; } = string.Empty;

        /// <summary>
        /// parent_id
        /// </summary>
        public int parent_id { get; set; } = 0;

        /// <summary>
        /// creator
        /// </summary>
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// is_valid
        /// </summary>
        public bool is_valid { get; set; } = true;

        /// <summary>
        /// tenant_id
        /// </summary>
        [Column("tenant_id")]
        public long TenantId { get; set; } = 1;
    }

        #endregion
}
