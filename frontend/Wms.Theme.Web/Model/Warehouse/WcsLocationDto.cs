namespace Wms.Theme.Web.Model.Warehouse;

public class LocationDto
{
    public string Address { get; set; } = "";
    public int? Level { get; set; } = 1;
    public string Type { get; set; } = "";
    public int? Status { get; set; } = 1;
    public int? CoordX { get; set; }
    public int? CoordY { get; set; }
    public int? CoordZ { get; set; }
    public int? StoragePriority { get; set; } = 1;
    public bool VirtualLocation { get; set; } = false;
}

public class StoreLocationDto : LocationDto
{
    public int Id { get; set; }
    public string PalletCode { get; set; } = "";
    public string PalletName { get; set; } = "";
    public List<ProductDto> Products { get; set; } = [];
}

public class ProductDto
{
    public int SkuId { get; set; }
    public string SkuCode { get; set; } = "";
    public string SkuName { get; set; } = "";
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public class WcsLocationDto : LocationDto
{
    public string Zone { get; set; } = "";
}
public class WcsBlockLocationDto
{
    public string Id { get; set; } = "";
    public string BlockCode { get; set; } = "";
    public string BlockName { get; set; } = "";
    public string Description { get; set; } = "";
}

public class CreateStoreSettingsRequest : StoreRuleSetting
{
    public int? WarehouseId { get; set; } = 1;
    public List<string> LocationIds { get; set; } = [];
}

public class StoreRuleSetting
{
    public int? SupplierId { get; set; }
    public int? BlockId { get; set; }
    public int? FloorId { get; set; }
    public int? SkuId { get; set; }
    public int? CategoryId { get; set; }
    public string RuleSettings { get; set; } = "FEFO";
}

public class StoreRuleSettingsDto : StoreRuleSetting
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = "";
    public string BlockName { get; set; } = "";
    public string FloorName { get; set; } = "";
    public string SkuName { get; set; } = "";
    public string CategoryName { get; set; } = "";
}

public class CreateStoreLocationRequest
{
    public int? WarehouseId { get; set; } = 1;
    public string BlockId { get; set; } = "";
    public IEnumerable<WcsLocationDto> Locations { get; set; } = [];
}

public class DeleteStoreSettingsRequest
{
    public int? SettingRuleId { get; set; } = 1;
}

public class LocationOnlyDto : LocationDto
{
    public int Id { get; set; }
}
