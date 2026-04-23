namespace WMSSolution.WMS.Entities.ViewModels.Stockadjust
{
    /// <summary>
    /// 
    /// </summary>
    public class StockSourceSelectionViewModel
    {

        /// <summary>
        /// 
        /// </summary>
        public int sku_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sku_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sku_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string spu_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string spu_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int goods_location_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LocationName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WarehouseName { get; set; }
        public string series_number { get; set; }
        public DateTime? expiry_date { get; set; }
        /// <summary>
        /// total quantity
        /// </summary>
        public int qty_total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int qty_locked { get; set; }
        public int qty_available { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int goods_owner_id { get; set; }
        public string goods_owner_name { get; set; }
        public decimal price { get; set; }
        public DateTime putaway_date { get; set; }
    }
}
