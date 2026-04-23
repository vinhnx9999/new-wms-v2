namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS
{
    /// <summary>
    /// Pallet Location
    /// </summary>
    public class PalletLocationResponse : BaseResponse
    {
        /// <summary>
        ///  item 
        /// </summary>
        public List<PalletLocationDto> Data { get; set; } = default!;

    }

    /// <summary>
    /// Pallet location DTO
    /// </summary>

    public class PalletLocationDto
    {
        /// <summary>
        /// Pallet Code 
        /// </summary>
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// Current Address
        /// </summary>  
        public string CurrentAddress { get; set; } = default!;
    }
}
