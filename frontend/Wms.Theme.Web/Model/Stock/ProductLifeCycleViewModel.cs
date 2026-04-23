namespace Wms.Theme.Web.Model.Stock
{
    public class ProductLifeCycleViewModel
    {
        public int sku_id { get; set; }
        public string series_number { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string DocNumber { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string sku_name { get; set; } = string.Empty;
        public string sku_code { get; set; } = string.Empty;
        public DateTime? PutAwayDate { get; set; }
        public string SeriesNumber { get; set; } = string.Empty;
    }
}
