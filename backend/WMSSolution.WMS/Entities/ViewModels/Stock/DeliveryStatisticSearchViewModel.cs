

using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// delivery data statistic input viewModel
    /// </summary>
    public class DeliveryStatisticSearchViewModel
    {
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public DeliveryStatisticSearchViewModel()
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
        /// customer_name
        /// </summary>
        [Display(Name = "customer_name")]
        [MaxLength(256, ErrorMessage = "MaxLength")]
        public string customer_name { get; set; } = string.Empty;

        /// <summary>
        /// Delivery date from
        /// </summary>
        [Display(Name = "delivery_date_from")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime delivery_date_from { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Delivery date to
        /// </summary>
        [Display(Name = "delivery_date_to")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime delivery_date_to { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// goods_owner_name
        /// </summary>
        [Display(Name = "goods_owner_name")]
        public string goods_owner_name { get; set; } = string.Empty;

        #endregion Property
    }
}