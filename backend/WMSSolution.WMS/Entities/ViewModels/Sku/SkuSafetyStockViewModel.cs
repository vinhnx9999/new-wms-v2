/*
 * date：2023-08-25
 * developer：AMo
 */
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// sku_safety_stock view model
    /// </summary>
    public class SkuSafetyStockViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public SkuSafetyStockViewModel()
        {

        }
        #endregion

        #region Property

        /// <summary>
        /// id
        /// </summary>
        [Display(Name = "id")]
        public int id { get; set; } = 0;

        /// <summary>
        /// sku id
        /// </summary>
        [Display(Name = "sku_id")]
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// warehouse's id
        /// </summary>
        [Display(Name = "WarehouseId")]
        public int WarehouseId { get; set; } = 0;

        /// <summary>
        /// WarehouseName
        /// </summary>
        [Display(Name = "WarehouseName")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        [Required(ErrorMessage = "Required")]
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// safety stock
        /// </summary>
        [Display(Name = "safety_stock_qty")]
        public int safety_stock_qty { get; set; } = 0;
        #endregion
    }
}
