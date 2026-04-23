using System.ComponentModel.DataAnnotations.Schema;
using WMSSolution.Core.Models;

namespace WMSSolution.WMS.Entities.Models.Approve
{
    /// <summary>
    /// 
    /// </summary>
    [Table("flowsetmain")]
    public class FlowSetMainEntity : BaseModel
    {
        /// <summary>
        /// menu
        /// </summary>
        public string menu { get; set; } = string.Empty;

        /// <summary>
        /// flowset name
        /// </summary>
        public string flow_name { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// tenant_id
        /// </summary>
        public long tenant_id { get; set; } = 1;

        /// <summary>
        /// flowset list
        /// </summary>
        public List<FlowSetEntity> flowset_list { get; set; } = new List<FlowSetEntity>();
    }
}