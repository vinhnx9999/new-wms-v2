namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound
{
    /// <summary>
    /// Create Inbound Task DTO
    /// </summary>
    public class CreateInboundTaskDTO
    {
        /// <summary>
        /// Pallet Codes
        /// </summary>
        public required string PalletCode { get; set; }
        /// <summary>
        /// Location ID
        /// </summary>
        public required int LocationId { get; set; }

        /// <summary>
        /// Pick Up Date
        /// </summary>
        public required DateTime PickUpDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public int Priority { get; set; } = 1;

    }
}
