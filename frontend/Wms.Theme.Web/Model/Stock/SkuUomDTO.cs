namespace Wms.Theme.Web.Model.Stock
{
    public class SkuUomDTO
    {
        public int Id { get; set; }
        public int SkuId { get; set; }
        public string UnitName { get; set; }
        public int ConversionRate { get; set; }
        public bool IsBaseUnit { get; set; }
        public long TenantId { get; set; }
    }
}
