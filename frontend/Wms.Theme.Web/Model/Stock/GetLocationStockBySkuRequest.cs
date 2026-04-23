namespace Wms.Theme.Web.Model.Stock
{
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
