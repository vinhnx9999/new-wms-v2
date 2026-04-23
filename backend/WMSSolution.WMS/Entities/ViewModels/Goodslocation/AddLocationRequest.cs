namespace WMSSolution.WMS.Entities.ViewModels.Goodslocation
{
    /// <summary>
    /// Add location request
    /// </summary>
    public class AddLocationRequest
    {
        /// <summary>
        /// Warehouse
        /// </summary>
        public int WarehouseId { get; set; } = default!;

        /// <summary>
        /// Location name
        /// </summary>
        public string? LocationName { get; set; }

        /// <summary>
        /// Coordinate X
        /// </summary>
        public string? CoordinateX { get; set; }

        /// <summary>
        /// Coordinate Y
        /// </summary>
        public string? CoordinateY { get; set; }

        /// <summary>
        /// Coordinate Z
        /// </summary>
        public string? CoordinateZ { get; set; }

        /// <summary>
        /// Is Virtual Location
        /// </summary>
        public bool IsVirtualLocation { get; set; } = default!;

        /// <summary>
        /// Priority
        /// </summary>
        public int Priority { get; set; } = default!;
    }
}
