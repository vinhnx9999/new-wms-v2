
namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// asn sort viewmodel
    /// </summary>
    public class AsnsortViewModel
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; } = 0;

        /// <summary>
        /// asn_id
        /// </summary>
        public int asn_id { get; set; } = 0;

        /// <summary>
        /// sorted_qty
        /// </summary>
        public int sorted_qty { get; set; } = 0;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// putaway qty
        /// </summary>
        public int putaway_qty { get; set; } = 0;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime expiry_date { get; set; } = DateTime.UtcNow;

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
        public long tenant_id { get; set; } = 1;
    }
}
