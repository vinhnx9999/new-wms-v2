namespace Wms.Theme.Web.Model.Warehouse;

public class WarehouseViewModel
{
    public int id { get; set; }
    public string WarehouseName { get; set; }
    public string city { get; set; }
    public string address { get; set; }
    public string email { get; set; }
    public string manager { get; set; }
    public string contact_tel { get; set; }
    public string creator { get; set; }
    public DateTime create_time { get; set; }
    public DateTime last_update_time { get; set; }
    public bool is_valid { get; set; }
    public long tenant_id { get; set; }
    public int LocationCount { get; set; }
    public int TotalInventory { get; set; }
    public int TotalPallet { get; set; }
    public string? WcsBlockId { get; set; }
}


public class AddWareHouseRequest
{
    public int Id { get; set; } = 0;
    public string WarehouseName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string ContactTel { get; set; } = string.Empty;
}

public class BasePostActionRequest
{
    public int Id { get; set; } = 0;
}