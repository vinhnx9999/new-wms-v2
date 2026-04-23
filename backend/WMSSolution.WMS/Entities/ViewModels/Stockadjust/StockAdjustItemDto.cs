using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels.Stockadjust
{
    public class StockAdjustItemDto
    {
        public int id { get; set; } = 0;

        /// <summary>
        /// spu_code
        /// </summary>
        public string spu_code { get; set; } = string.Empty;

        /// <summary>
        /// spu_name
        /// </summary>
        public string spu_name { get; set; } = string.Empty;

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// sku_code
        /// </summary>
        public string sku_code { get; set; } = string.Empty;

        /// <summary>
        /// sku_name
        /// </summary>
        public string sku_name { get; set; } = string.Empty;

        /// <summary>
        /// goods_owner_id
        /// </summary>
        public int goods_owner_id { get; set; } = 0;

        /// <summary>
        /// goods owner's name
        /// </summary>
        public string goods_owner_name { get; set; } = string.Empty;

        /// <summary>
        /// goods_location_id
        /// </summary>
        [Display(Name = "goods_location_id")]
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// WarehouseName
        /// </summary>
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// LocationName
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// qty
        /// </summary>
        public int qty { get; set; } = 0;

        /// <summary>
        /// creator
        /// </summary>
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; }
        /// <summary>
        /// tenant_id
        /// </summary>
        [Display(Name = "tenant_id")]
        public long tenant_id { get; set; } = 0;

        /// <summary>
        /// is_update_stock
        /// </summary>
        [Display(Name = "is_update_stock")]
        public bool is_update_stock { get; set; } = false;

        /// <summary>
        /// source_table_id
        /// </summary>
        [Display(Name = "source_table_id")]
        public int source_table_id { get; set; } = 0;

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
