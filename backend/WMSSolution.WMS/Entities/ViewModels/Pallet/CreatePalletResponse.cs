namespace WMSSolution.WMS.Entities.ViewModels.Pallet
{
    /// <summary>
    /// pallet response 
    /// </summary>
    public class CreatePalletResponse
    {
        /// <summary>
        /// Pallet Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pallet Code
        /// </summary>

        public string PalletCode { get; set; } = default!;
    }
}
