
using System.ComponentModel.DataAnnotations;
using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// pallet viewModel
    /// </summary>
    public class PalletViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public PalletViewModel()
        {
        }
        #endregion

        #region Property
        /// <summary>
        /// id
        /// </summary>
        [Display(Name = "id")]
        public int Id { get; set; } = 0;

        /// <summary>
        /// PalletCode
        /// </summary>
        [Display(Name = "PalletCode")]
        [Required(ErrorMessage = "Required")]
        public string PalletCode { get; set; } = string.Empty;

        /// <summary>
        /// PalletStatus
        /// </summary>
        [Display(Name = "PalletStatus")]
        public PalletEnumStatus PalletStatus { get; set; } = PalletEnumStatus.Available;

        /// <summary>
        /// IsFull
        /// </summary>
        [Display(Name = "IsFull")]
        public bool IsFull { get; set; } = false;

        /// <summary>
        /// IsMixed
        /// </summary>
        [Display(Name = "IsMixed")]
        public bool IsMixed { get; set; } = true;

        /// <summary>
        /// MaxWeight
        /// </summary>
        [Display(Name = "MaxWeight")]
        public decimal MaxWeight { get; set; } = 0;

        /// <summary>
        /// CurrentWeight
        /// </summary>
        [Display(Name = "CurrentWeight")]
        public decimal CurrentWeight { get; set; } = 0;

        /// <summary>
        /// Length
        /// </summary>
        [Display(Name = "Length")]
        public decimal Length { get; set; } = 0;

        /// <summary>
        /// Width
        /// </summary>
        [Display(Name = "Width")]
        public decimal Width { get; set; } = 0;

        /// <summary>
        /// Height
        /// </summary>
        [Display(Name = "Height")]
        public decimal Height { get; set; } = 0;

        /// <summary>
        /// PalletType
        /// </summary>
        [Display(Name = "PalletType")]
        public string? PalletType { get; set; }
        #endregion
    }
}

