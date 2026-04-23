namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    public class ProductLifeCycleViewModel
    {
        public int sku_id { get; set; }
        public string series_number { get; set; }
        public string ActivityType { get; set; }
        public DateTime EventTime { get; set; }
        public string DocNumber { get; set; }
        public string PartnerName { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public int Qty { get; set; }
        public string sku_name { get; set; }
        public string sku_code { get; set; }
        public DateTime? PutAwayDate { get; set; }
    }
}
