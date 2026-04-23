

namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// safety stock mangement viewmodel
    /// </summary>
    public class SafetyStockManagementViewModel
    {
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public SafetyStockManagementViewModel()
        {
        }

        #endregion constructor

        #region Property

        /// <summary>
        /// WarehouseName
        /// </summary>
        public string WarehouseName { get; set; } = string.Empty;

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
        /// sku_name
        /// </summary>
        public string sku_name { get; set; } = string.Empty;

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// quantity
        /// </summary>
        public int qty { get; set; } = 0;

        /// <summary>
        /// quantity available
        /// </summary>
        public int qty_available { get; set; } = 0;

        /// <summary>
        /// quantity locked
        /// </summary>
        public int qty_locked { get; set; } = 0;

        /// <summary>
        /// quantity frozen
        /// </summary>
        public int qty_frozen { get; set; } = 0;

        /// <summary>
        /// safety_stock_qty
        /// </summary>
        public int safety_stock_qty { get; set; } = 0;

        #endregion Property
    }
}