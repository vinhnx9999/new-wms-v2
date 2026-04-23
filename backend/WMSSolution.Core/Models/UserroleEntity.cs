
using System.ComponentModel.DataAnnotations.Schema;

namespace WMSSolution.Core.Models
{
    /// <summary>
    /// userrole  entity
    /// </summary>
    [Table("userrole")]
    public class UserroleEntity : BaseModel
    {

        #region Property

        /// <summary>
        /// role_name
        /// </summary>
        public string role_name { get; set; } = string.Empty;

        /// <summary>
        /// is_valid
        /// </summary>
        public bool is_valid { get; set; } = false;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant_id
        /// </summary>
        public long tenant_id { get; set; } = 0;


        #endregion

    }
}
