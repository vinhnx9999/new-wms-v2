using Microsoft.AspNetCore.Mvc.Rendering;

namespace Wms.Theme.Web.Model.Warehouse;

public class WarehouseGeneralInfo : WarehouseDTO
{
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public int LocationCount { get; set; }
    public int TotalInventory { get; set; }
    public int TotalPallet { get; set; }
    public int ProcessingOrders { get; set; }
    public int CapacityPercent
    {
        get
        {
            return LocationCount == 0 ? 0 :
                (int)Math.Round((decimal)TotalInventory / LocationCount * 100, 2);
        }
    }
    
    public string? WcsBlockId { get; set; }
}

public class WarehouseLocationInfo
{
    public IEnumerable<int> Floors { get; set; } = [];
    public IEnumerable<StoreLocationDto> StoreLocations { get; set; } = [];
    public int FloorId { get; set; }
    public List<SelectListItem> OptionFloors
    {
        get
        {
            var rs = new List<SelectListItem>();
            foreach (var floorId in Floors)
            {
                rs.Add(new SelectListItem
                {
                    Value = $"{floorId}",
                    Text = floorId == 1 ? "First Floor" : $"Floor {floorId}"
                });
            }
            return rs;
        }
    }
}

public class WarehouseRuleInfo
{
    public IEnumerable<StoreRuleSettingsDto> RuleSettings { get; set; } = [];
    public IEnumerable<int> Floors { get; set; } = [];
    public int SupplierId { get; set; }
    public string RuleCode { get; set; } = "FIFO";
    public int SkuId { get; set; }
    public int FloorId { get; set; }

    public List<SelectListItem> OptionFloors
    {
        get
        {
            var rs = new List<SelectListItem>();
            foreach (var floorId in Floors)
            {
                rs.Add(new SelectListItem
                {
                    Value = $"{floorId}",
                    Text = floorId == 1 ? "First Floor" : $"Floor {floorId}"
                });
            }
            return rs;
        }
    }
}