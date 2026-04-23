

namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS
{
    /// <summary>
    /// Sync Location
    /// </summary>
    public class SyncLocationRequest
    {
        /// <summary>
        /// Block Id
        /// </summary>
        public string BlockId { get; set; } = default!;

        /// <summary>
        /// Pallet Locations
        /// </summary>
        public List<Warehouse.PalletLocationSync> PalletLocations { get; set; } = default!;

        /// <summary>
        /// Action time when adding this batch in WCS
        /// </summary>
        public DateTime ActionTime { get; set; } = default!;

    }

}
