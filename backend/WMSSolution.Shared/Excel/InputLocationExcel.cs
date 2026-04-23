namespace WMSSolution.Shared.Excel;

public class InputLocationExcel
{
    public string WarehouseName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string CoordinateX { get; set; } = string.Empty;
    public string CoordinateY { get; set; } = string.Empty;
    public string CoordinateZ { get; set; } = string.Empty;
    public bool IsVirtualLocation { get; set; }
    public int Priority { get; set; } = 1;
}
