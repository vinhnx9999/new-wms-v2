namespace WMSSolution.WMS.Entities.ViewModels.Pallet
{
    /// <summary>
    ///  Dto pallet page search
    /// </summary>
    public class PalletPageSearchDTO
    {
        /// <summary>
        /// pallet id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// pallet Code 
        /// </summary>
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// create time 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Pallet status
        /// </summary>
        public int PalletStatus { get; set; }

    }
}
