

using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// goods  stock age management viewmodel
    /// </summary>
    public class StockAgeViewModel
    {
        /// <summary>
        /// WarehouseName
        /// </summary>
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// LocationName
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

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
        /// quantity
        /// </summary>
        public int qty { get; set; } = 0;


        /// <summary>
        /// goods owner name
        /// </summary>
        public string goods_owner_name { get; set; } = string.Empty;

        /// <summary>
        /// series_number
        /// </summary>
        [Display(Name = "series_number")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// goods_location_id
        /// </summary>
        public int goods_location_id { get; set; } = 0;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime? expiry_date { get; set; } 

        /// <summary>
        /// price
        /// </summary>
        public decimal price { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        public DateTime putaway_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// stock_age
        /// </summary>
        public int stock_age { get; set; } = 0;

    }
}