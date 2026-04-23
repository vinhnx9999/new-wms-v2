namespace Wms.Theme.Web.Model.StockTaking
{
    public class StocktakingViewModel
    {
        public int id { get; set; } = 0;
        public string job_code { get; set; } = string.Empty;
        public bool job_status { get; set; } = false;
        public bool adjust_status { get; set; } = false;
        public int counted_qty { get; set; } = 0;
        public int difference_qty { get; set; } = 0;
        public string creator { get; set; } = string.Empty;
        public DateTime create_time { get; set; }
        public string handler { get; set; } = string.Empty;
        public DateTime handle_time { get; set; }
        public DateTime last_update_time { get; set; }
    }
}
