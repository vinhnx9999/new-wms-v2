
using System.ComponentModel.DataAnnotations;


namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// stocktaking basic viewModel
    /// </summary>
    public class StocktakingBasicViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public StocktakingBasicViewModel()
        {

        }
        #endregion
        #region Property

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
        [Display(Name = "sku_id")]
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
        [Display(Name = "goods_owner_id")]
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
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime expiry_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// price
        /// </summary>
        public decimal price { get; set; } = 0;

        /// <summary>
        /// book_qty
        /// </summary>
        [Display(Name = "book_qty")]
        public int book_qty { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        public DateTime putaway_date { get; set; } = DateTime.UtcNow;

        #endregion
    }
}
