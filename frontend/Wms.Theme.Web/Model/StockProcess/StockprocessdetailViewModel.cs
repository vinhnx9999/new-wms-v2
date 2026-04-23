namespace Wms.Theme.Web.Model.StockProcess
{
    public class StockprocessdetailViewModel
    {

        public int id { get; set; } = 0;

        /// <summary>
        /// stock_process_id
        /// </summary>
        public int stock_process_id { get; set; } = 0;

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// goods_location_id
        /// </summary>
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// qty
        /// </summary>
        public int qty { get; set; } = 0;

        /// <summary>
        /// last_update_time
        /// </summary>

        public DateTime last_update_time { get; set; }

        /// <summary>
        /// tenant_id
        /// </summary>
        public long tenant_id { get; set; } = 0;

        /// <summary>
        /// is_source
        /// </summary>
        public bool is_source { get; set; } = true;

        /// <summary>
        /// spu_code
        /// </summary>
        public string spu_code { get; set; } = string.Empty;

        /// <summary>
        /// spu_name
        /// </summary>
        public string spu_name { get; set; } = string.Empty;

        /// <summary>
        /// sku_code
        /// </summary>
        public string sku_code { get; set; } = string.Empty;

        /// <summary>
        /// unit
        /// </summary>
        public string unit { get; set; } = string.Empty;

        /// <summary>
        /// is_update_stock
        /// </summary>
        public bool is_update_stock { get; set; } = false;

        /// <summary>
        /// goods location name
        /// </summary>
        public string location_name { get; set; } = string.Empty;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime expiry_date { get; set; }

        /// <summary>
        /// price
        /// </summary>
        public decimal price { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        public DateTime putaway_date { get; set; }

    }
}
