using System.Text.Json.Serialization;

namespace Wms.Theme.Web.Model.GoodLocation
{
    public class GoodLocationDto
    {
        public int Id { get; set; } = 0;
        public int WarehouseId { get; set; } = 0;
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseAreaName { get; set; } = string.Empty;
        public byte WarehouseAreaProperty { get; set; } = 0;
        public string LocationName { get; set; } = string.Empty;
        public decimal LocationLength { get; set; } = 0;
        public decimal LocationWidth { get; set; } = 0;
        public decimal LocationHeigth { get; set; } = 0;
        public decimal LocationVolume { get; set; } = 0;
        public decimal LocationLoad { get; set; } = 0;
        public string CoordinateX { get; set; } = string.Empty;
        public string CoordinateY { get; set; } = string.Empty;
        public string CoordinateZ { get; set; } = string.Empty;
        public byte LocationStatus { get; set; } = 0;
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
        public bool IsValid { get; set; } = true;
        public long TenantId { get; set; } = 0;
        public int WarehouseAreaId { get; set; } = 0;
    }
}