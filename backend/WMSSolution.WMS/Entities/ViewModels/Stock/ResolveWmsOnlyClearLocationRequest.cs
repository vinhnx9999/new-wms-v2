namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// Resolve WMS only clear location request
    /// </summary>
    public class ResolveWmsOnlyClearLocationRequest
    {
        /// <summary>
        /// Warehouse id.
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Location name in WMS.
        /// </summary>
        public string LocationName { get; set; } = default!;

        /// <summary>
        /// Optional resolution note.
        /// </summary>
        public string? Note { get; set; }
    }
}
