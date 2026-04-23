
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// asn flow confirm input viewModel
    /// </summary>
    public class AsnConfirmInputViewModel
    {

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public AsnConfirmInputViewModel()
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
        /// input_qty
        /// </summary>
        [Display(Name = "input_qty")]
        [JsonPropertyName("input_qty")]
        public int input_qty { get; set; } = 0;

        /// <summary>
        /// arrival_time
        /// </summary>
        [Display(Name = "arrival_time")]
        public DateTime arrival_time { get; set; } = DateTime.UtcNow;
        #endregion
    }
}
