namespace Wms.Theme.Web.Model.StockProcess
{
    public class StockprocessGetViewModel
    {
        public int id { get; set; } = 0;
        public int qty { get; set; } = 0;
        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;
        public string job_code { get; set; } = string.Empty;

        public bool job_type { get; set; } = true;

        public bool process_status { get; set; } = true;

        public string processor { get; set; } = string.Empty;

        public DateTime? process_time { get; set; }

        public string creator { get; set; } = string.Empty;

        public DateTime create_time { get; set; }
        public DateTime last_update_time { get; set; }
        public long tenant_id { get; set; } = 0;
        public bool adjust_status { get; set; } = true;
    }
}
