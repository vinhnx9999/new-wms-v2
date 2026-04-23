/*
 * date：2022-12-21
 * developer：NoNo
 */
using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// warehousearea  entity
    /// </summary>
    [Table("warehousearea")]
    public class WarehouseareaEntity : BaseModel
    {

        #region Property

        /// <summary>
        /// WarehouseId
        /// </summary>
        public int WarehouseId { get; set; } = 0;

        /// <summary>
        /// area_name
        /// </summary>
        public string area_name { get; set; } = string.Empty;

        /// <summary>
        /// parent_id
        /// </summary>
        public int parent_id { get; set; } = 0;

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
        public bool is_valid { get; set; } = false;

        /// <summary>
        /// tenant_id
        /// </summary>
        public long tenant_id { get; set; } = 0;

        /// <summary>
        /// area_property
        /// </summary>
        public byte area_property { get; set; } = 0;


        #endregion

    }
}
