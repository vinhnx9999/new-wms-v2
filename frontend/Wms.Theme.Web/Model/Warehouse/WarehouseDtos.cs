namespace Wms.Theme.Web.Model.Warehouse;

public class WarehouseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
}

public class WarehouseRuleDTO
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
}

public class WarehouseDetailDTO
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int BlockId { get; set; }
    public int FloorId { get; set; }
    public int SkuId { get; set; }
    public int CategoryId { get; set; }
    public string RuleId { get; set; } = "FEFO";
    public List<int> LocationIds { get; set; } = [];
    public List<string> LocationNames { get; set; } = [];

}


