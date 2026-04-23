namespace Wms.Theme.Web.Model.Stock
{
    public class InventoryDashboardViewModel
    {
        public int total_sku { get; set; }
        public decimal total_stock_qty { get; set; }
        public int used_locations { get; set; }
        public int total_locations { get; set; }
        public double location_usage_percent => total_locations == 0 ? 0 : Math.Round((double)used_locations / total_locations * 100, 1);
    }
}
