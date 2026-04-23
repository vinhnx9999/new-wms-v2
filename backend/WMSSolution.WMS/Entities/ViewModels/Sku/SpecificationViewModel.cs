using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMSSolution.WMS.Entities.ViewModels.Sku
{
    public class SpecificationViewModel
    {
        #region constructor
        public SpecificationViewModel()
        {
        }
        #endregion

        /// <summary>
        /// id
        /// </summary>
        #region Property
        [Display(Name = "id")]
        public int id { get; set; } = 0;

        /// <summary>
        /// specification_name
        /// </summary>
        [Display(Name = "specification_name")]
        [Required(ErrorMessage = "Required")]
        [MaxLength(32, ErrorMessage = "MaxLength")]
        public string specification_name { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        [Display(Name = "creator")]
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        [Display(Name = "create_time")]
        [DataType(DataType.DateTime, ErrorMessage = "DataType_DateTime")]
        public DateTime create_time { get; set; } = DateTime.UtcNow;
        #endregion
    }
}
