namespace WMSSolution.WMS.Entities.ViewModels.Stock
{
    /// <summary>
    /// View model for get location stock by sku request
    /// </summary>
    public class GetLocationStockBySkuRequest
    {
        /// <summary>
        /// warehouseId 
        /// </summary>
        public int WarehouseId { get; set; } = 0;
        /// <summary>
        /// sku id 
        /// </summary>
        public int SkuId { get; set; } = 0;
    }
}
