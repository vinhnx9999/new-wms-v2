
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// warehousearea viewModel
    /// </summary>
    public class WarehouseareaViewModel
    {

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public WarehouseareaViewModel()
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
        /// WarehouseId
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
        /// area_name
        /// </summary>
        [Display(Name = "area_name")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        [Required(ErrorMessage = "Required")]
        public string area_name { get; set; } = string.Empty;

        /// <summary>
        /// parent_id
        /// </summary>
        [Display(Name = "parent_id")]
        public int parent_id { get; set; } = 0;

        /// <summary>
        /// create_time
        /// </summary>
        [Display(Name = "create_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime create_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last_update_time
        /// </summary>
        [Display(Name = "last_update_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime last_update_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// is_valid
        /// </summary>
        [Display(Name = "is_valid")]
        public bool is_valid { get; set; } = true;

        /// <summary>
        /// tenant_id
        /// </summary>
        [Display(Name = "tenant_id")]
        public long tenant_id { get; set; } = 0;

        /// <summary>
        /// area_property
        /// </summary>
        [Display(Name = "area_property")]
        public byte area_property { get; set; } = 0;


        #endregion

    }
}
