
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// userrole viewModel
    /// </summary>
    public class UserroleViewModel
    {

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public UserroleViewModel()
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
        /// role_name
        /// </summary>
        [Display(Name = "role_name")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string role_name { get; set; } = string.Empty;

        /// <summary>
        /// is_valid
        /// </summary>
        [Display(Name = "is_valid")]
        public bool is_valid { get; set; } = false;

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
        /// tenant_id
        /// </summary>
        [Display(Name = "tenant_id")]
        public long tenant_id { get; set; } = 0;


        #endregion

    }
}
