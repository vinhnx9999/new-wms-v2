/*
 * date：2022-12-26
 * developer：NoNo
 */

using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;

namespace WMSSolution.WMS.Entities.Models
{
    /// <summary>
    /// stockfreeze  entity
    /// </summary>
    [Table("stockfreeze")]
    public class StockfreezeEntity : BaseModel
    {
        #region Property

        /// <summary>
        /// job_code
        /// </summary>
        public string job_code { get; set; } = string.Empty;

        /// <summary>
        /// job_type
        /// </summary>
        public bool job_type { get; set; } = false;

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// goods_location_id
        /// </summary>
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// handler
        /// </summary>
        public string handler { get; set; } = string.Empty;

        /// <summary>
        /// operate_time
        /// </summary>
        public DateTime handle_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant_id
        /// </summary>
        public long tenant_id { get; set; } = 0;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        #endregion Property
    }
}