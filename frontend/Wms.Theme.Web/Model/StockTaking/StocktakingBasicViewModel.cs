namespace Wms.Theme.Web.Model.StockTaking
{
    public class StocktakingBasicViewModel
    {
        public string spu_code { get; set; } = string.Empty;
        public string spu_name { get; set; } = string.Empty;
        public int sku_id { get; set; } = 0;
        public string sku_code { get; set; } = string.Empty;
        public string sku_name { get; set; } = string.Empty;
        public int goods_owner_id { get; set; } = 0;
        public string goods_owner_name { get; set; } = string.Empty;
        public int goods_location_id { get; set; } = 0;
        public string warehouse_name { get; set; } = string.Empty;
        public string location_name { get; set; } = string.Empty;
        public string series_number { get; set; } = string.Empty;
        public DateTime expiry_date { get; set; }
        public decimal price { get; set; } = 0;
        public int book_qty { get; set; } = 0;
        public DateTime putaway_date { get; set; }
    }
}
