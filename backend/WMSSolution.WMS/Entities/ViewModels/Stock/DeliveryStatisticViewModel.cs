using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// delivery data statistic viewmodel
    /// </summary>
    public class DeliveryStatisticViewModel
    {
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public DeliveryStatisticViewModel()
        {
        }

        #endregion constructor

        #region Property

        /// <summary>
        /// dispatch_no
        /// </summary>
        public string dispatch_no { get; set; } = string.Empty;

        /// <summary>
        /// WarehouseName
        /// </summary>
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// LocationName
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// sku_code
        /// </summary>
        public string sku_code { get; set; } = string.Empty;

        /// <summary>
        /// sku_name
        /// </summary>
        public string sku_name { get; set; } = string.Empty;

        /// <summary>
        /// spu_code
        /// </summary>
        public string spu_code { get; set; } = string.Empty;

        /// <summary>
        /// spu_name
        /// </summary>
        public string spu_name { get; set; } = string.Empty;

        /// <summary>
        /// customer_name
        /// </summary>
        public string customer_name { get; set; } = string.Empty;

        /// <summary>
        /// series_number
        /// </summary>

        public string series_number { get; set; } = string.Empty;

        /// <summary>
        /// delivery qty
        /// </summary>
        public int delivery_qty { get; set; } = 0;

        /// <summary>
        /// Delivery date
        /// </summary>
        [Display(Name = "delivery_date")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime delivery_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// goods owner name
        /// </summary>
        public string goods_owner_name { get; set; } = string.Empty;

        /// <summary>
        /// delivery amount
        /// </summary>
        public decimal delivery_amount { get; set; } = 0;

        /// <summary>
        /// expiry_date
        /// </summary>
        public DateTime expiry_date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// price
        /// </summary>
        public decimal price { get; set; } = 0;

        /// <summary>
        /// putaway_date
        /// </summary>
        public DateTime putaway_date { get; set; } = DateTime.UtcNow;


        #endregion Property
    }
}