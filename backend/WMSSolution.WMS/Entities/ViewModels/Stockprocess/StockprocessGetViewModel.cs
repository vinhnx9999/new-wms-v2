
using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// stockprocess viewModel without details
    /// </summary>
    public class StockprocessGetViewModel
    {

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public StockprocessGetViewModel()
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
        /// job_code
        /// </summary>
        [Display(Name = "job_code")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string job_code { get; set; } = string.Empty;

        /// <summary>
        /// job_type
        /// </summary>
        [Display(Name = "job_type")]
        public bool job_type { get; set; } = true;

        /// <summary>
        /// process_status
        /// </summary>
        [Display(Name = "process_status")]
        public bool process_status { get; set; } = true;

        /// <summary>
        /// processor
        /// </summary>
        [Display(Name = "processor")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string processor { get; set; } = string.Empty;

        /// <summary>
        /// process_time
        /// </summary>
        [Display(Name = "process_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime? process_time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// creator
        /// </summary>
        [Display(Name = "creator")]
        [MaxLength(64, ErrorMessage = "MaxLength")]
        public string creator { get; set; } = string.Empty;

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

        /// <summary>
        /// adjust_status
        /// </summary>
        public bool adjust_status { get; set; } = true;

        /// <summary>
        /// qty
        /// </summary>
        [Display(Name = "qty")]
        public int qty { get; set; } = 0;

        /// <summary>
        /// series_number
        /// </summary>
        public string series_number { get; set; } = string.Empty;

        #endregion
    }
}
