namespace Wms.Theme.Web.Model.StockMove
{
    public class StockmoveViewModel
    {
        public int id { get; set; } = 0;
        public string job_code { get; set; } = string.Empty;
        public byte move_status { get; set; } = 0;
        public int sku_id { get; set; } = 0;
        public int orig_goods_location_id { get; set; } = 0;
        public int dest_googs_location_id { get; set; } = 0;
        public int qty { get; set; } = 0;
        public int goods_owner_id { get; set; } = 0;
        public string handler { get; set; } = string.Empty;
        public DateTime handle_time { get; set; }
        public string creator { get; set; } = string.Empty;
        public DateTime create_time { get; set; }
        public DateTime last_update_time { get; set; }
        public long tenant_id { get; set; } = 0;
        public string orig_goods_warehouse { get; set; } = string.Empty;
        public string orig_goods_location_name { get; set; } = string.Empty;
        public string dest_googs_warehouse { get; set; } = string.Empty;
        public string dest_googs_location_name { get; set; } = string.Empty;
        public string spu_code { get; set; } = string.Empty;
        public string spu_name { get; set; } = string.Empty;
        public string sku_code { get; set; } = string.Empty;
        public string sku_name { get; set; } = string.Empty;
        public string series_number { get; set; } = string.Empty;
        public DateTime expiry_date { get; set; }
        public decimal price { get; set; } = 0;
        public DateTime putaway_date { get; set; }

    }
}
