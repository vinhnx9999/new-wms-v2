namespace Wms.Theme.Web.Model.GoodLocation;

public class AddLocationRequest
{
    public int WarehouseId { get; set; }
    public string? LocationName { get; set; }
    public string? CoordinateX { get; set; }
    public string? CoordinateY { get; set; }
    public string? CoordinateZ { get; set; }
    public bool IsVirtualLocation { get; set; }
    public int Priority { get; set; } = 1;
}