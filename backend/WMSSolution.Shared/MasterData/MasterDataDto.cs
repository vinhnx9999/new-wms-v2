using WMSSolution.Shared.Enums.Location;

namespace WMSSolution.Shared.MasterData;

public class MasterDataDto
{
    public IEnumerable<SkuMaster> Skus { get; set; } = [];
    public IEnumerable<LocationMaster> Locations { get; set; } = [];
    public IEnumerable<ChainMaster> Suppliers { get; set; } = [];
    public IEnumerable<ChainMaster> Customers { get; set; } = [];
}

public class LocationMaster
{
    public int WarehouseId { get; set; }
    public int LocationId { get; set; }
    public GoodsLocationTypeEnum? LocationType { get; set; } = GoodsLocationTypeEnum.None;
    public string? LocationName { get; set; } = "";
}

public class SkuMaster
{
    public int SkuId { get; set; }
    public string? SkuCode { get; set; } = "";
    public string? SkuName { get; set; } = "";
}

public class ChainMaster
{
    public int Id { get; set; }
    public string? Code { get; set; } = "";
    public string? Name { get; set; } = "";
}

public class VendorMaster 
{
    public long Id { get; set; }
    public string? Company { get; set; } = "";
    public string? ContactName { get; set; } = "";
    public string? ContactNumber { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidTo { get; set; } = DateTime.UtcNow;
}
