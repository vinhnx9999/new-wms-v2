namespace Wms.Theme.Web.Model.StockAdjust
{
    public class StockSourceSelectionViewModel
    {
        public int sku_id { get; set; }
        public string sku_code { get; set; }
        public string sku_name { get; set; }
        public string spu_code { get; set; }
        public string spu_name { get; set; }
        public int goods_location_id { get; set; }
        public string location_name { get; set; }
        public string warehouse_name { get; set; }
        public string series_number { get; set; }
        public DateTime? expiry_date { get; set; }
        public int qty_total { get; set; }
        public int qty_locked { get; set; }
        public int qty_available { get; set; }
        public int goods_owner_id { get; set; }
        public string goods_owner_name { get; set; }
        public decimal price { get; set; }
        public DateTime putaway_date { get; set; }
    }
}
