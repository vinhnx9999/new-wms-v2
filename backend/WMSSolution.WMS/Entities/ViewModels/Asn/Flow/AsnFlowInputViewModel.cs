/*
 * date：2022-12-22
 * developer：AMo
 */
using System;
using System.ComponentModel.DataAnnotations;
using WMSSolution.Core.Utility;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// asn flow input viewModel
    /// </summary>
    public class AsnFlowInputViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public AsnFlowInputViewModel()
        {

        }
        #endregion

        #region Property

        /// <summary>
        /// id
        /// </summary>
        [Display(Name = "id")]
        public int id { get; set; } = 0;

        #endregion
    }
}
