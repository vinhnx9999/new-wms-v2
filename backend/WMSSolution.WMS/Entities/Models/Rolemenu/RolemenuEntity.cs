
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// rolemenu  entity
    /// </summary>
    [Table("rolemenu")]
    public class RolemenuEntity : BaseModel
    {

        #region Property

        /// <summary>
        /// userrole_id
        /// </summary>
        public int userrole_id { get; set; } = 0;

        /// <summary>
        /// menu_id
        /// </summary>
        public int menu_id { get; set; } = 0;

        /// <summary>
        /// authority
        /// </summary>
        public byte authority { get; set; } = 1;

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
        public long tenant_id { get; set; } = 1;

        /// <summary>
        /// actions authority
        /// </summary>
        public string menu_actions_authority { get; set; } = "[]";
        #endregion

    }
}
