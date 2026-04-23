namespace Wms.Theme.Web.Model.Stock
{
    /// <summary>
    /// StockManagementViewModel
    /// </summary>
    public class StockManagementViewModel
    {
        public string spu_code { get; set; } = string.Empty;
        public string spu_name { get; set; } = string.Empty;
        public string sku_code { get; set; } = string.Empty;
        public int sku_id { get; set; } = 0;
        public string sku_name { get; set; } = string.Empty;
        public int qty { get; set; } = 0;
        public int qty_available { get; set; } = 0;
        public int qty_locked { get; set; } = 0;
        public int qty_frozen { get; set; } = 0;
        public int qty_asn { get; set; } = 0;
        public int qty_to_unload { get; set; } = 0;
        public int qty_to_sort { get; set; } = 0;
        public int qty_sorted { get; set; } = 0;
        public int shortage_qty { get; set; } = 0;
        public DateTime expiry_date { get; set; } = DateTime.UtcNow;
        public string warehouse_name { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public string unit { get; set; } = string.Empty;      
        public string location_name { get; set; } = string.Empty;
        public string series_number { get; set; } = string.Empty;
    }
}
