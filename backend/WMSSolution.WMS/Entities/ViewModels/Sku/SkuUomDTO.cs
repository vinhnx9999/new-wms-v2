using System;

namespace WMSSolution.WMS.Entities.ViewModels
{
    public class SkuUomDTO
    {
        public int Id { get; set; }
        public int SkuId { get; set; }
        public string UnitName { get; set; }
        public long TenantId { get; set; }
        public string? Description { get; set; }
    }
}
