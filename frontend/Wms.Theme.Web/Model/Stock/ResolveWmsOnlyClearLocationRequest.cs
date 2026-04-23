namespace Wms.Theme.Web.Model.Stock
{
    public class ResolveWmsOnlyClearLocationRequest
    {
        public int WarehouseId { get; set; }
        public string LocationName { get; set; } = default!;
        public string? Note { get; set; }
    }
}