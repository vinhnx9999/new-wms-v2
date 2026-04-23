using System.ComponentModel.DataAnnotations;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class FlowSetUserViewModel
    {
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public FlowSetUserViewModel()
        {
        }

        #endregion constructor

        #region Property

        /// <summary>
        /// id
        /// </summary>
        [Display(Name = "id")]
        public int id { get; set; } = 0;

        /// <summary>
        /// menu
        /// </summary>
        [Display(Name = "menu")]
        public string menu { get; set; } = string.Empty;

        /// <summary>
        /// flowset id
        /// </summary>
        [Display(Name = "flowset_id")]
        public int flowset_id { get; set; } = 0;

        /// <summary>
        /// node guid
        /// </summary>
        [Display(Name = "node_guid")]
        public string node_guid { get; set; } = string.Empty;

        /// <summary>
        /// user id
        /// </summary>
        [Display(Name = "user_id")]
        public int user_id { get; set; } = 0;

        /// <summary>
        /// user name
        /// </summary>
        [Display(Name = "user_name")]
        public string user_name { get; set; } = string.Empty;

        #endregion Property
    }
}