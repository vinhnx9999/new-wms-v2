namespace Wms.Theme.Web.Model.StockProcess
{
    public class StockprocessWithDetailViewModel
    {
        public int id { get; set; } = 0;
        public string job_code { get; set; } = string.Empty;
        public bool job_type { get; set; } = true;
        public bool process_status { get; set; } = true;
        public string processor { get; set; } = string.Empty;

        public DateTime process_time { get; set; }
        public string creator { get; set; } = string.Empty;
        public DateTime create_time { get; set; }

        public DateTime last_update_time { get; set; }
        public long tenant_id { get; set; } = 0;

        public bool adjust_status { get; set; } = true;


        /// <summary>
        /// source detail table
        /// </summary>
        public List<StockprocessdetailViewModel> source_detail_list { get; set; } = new List<StockprocessdetailViewModel>(2);
        /// <summary>
        /// target detail table
        /// </summary>
        public List<StockprocessdetailViewModel> target_detail_list { get; set; } = new List<StockprocessdetailViewModel>(2);
    }
}

