

using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// delivery data statistic input viewModel
    /// </summary>
    public class StockAgeSearchViewModel
    {
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public StockAgeSearchViewModel()
        {
        }

        #endregion constructor

        #region Property

        /// <summary>
        /// current page number
        /// </summary>
        public int pageIndex { get; set; } = 1;

        /// <summary>
        /// rows per page
        /// </summary>
        public int pageSize { get; set; } = 20;

        /// <summary>
        /// spu_code
        /// </summary>
        [Display(Name = "spu_code")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string spu_code { get; set; } = string.Empty;

        /// <summary>
        /// spu_name
        /// </summary>
        [Display(Name = "spu_name")]
        [MaxLength(200, ErrorMessage = "MaxLength")]
        public string spu_name { get; set; } = string.Empty;

        /// <summary>
        /// sku_code
        /// </summary>
        [Display(Name = "sku_code")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string sku_code { get; set; } = string.Empty;

        /// <summary>
        /// sku_name
        /// </summary>
        [Display(Name = "sku_name")]
        [MaxLength(200, ErrorMessage = "MaxLength")]
        public string sku_name { get; set; } = string.Empty;

        /// <summary>
        /// WarehouseName
        /// </summary>
        [Display(Name = "WarehouseName")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// LocationName
        /// </summary>
        [Display(Name = "LocationName")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// stock age date from
        /// </summary>
        public int stock_age_from { get; set; } = 0;

        /// <summary>
        /// stock age date to
        /// </summary>
        public int stock_age_to { get; set; } = 0;


        /// <summary>
        ///expiry date from
        /// </summary>
        [Display(Name = "expiry_date_from")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime expiry_date_from { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// expiry date to
        /// </summary>
        [Display(Name = "expiry_date_to")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime expiry_date_to { get; set; } = DateTime.UtcNow;
        #endregion Property
    }
}