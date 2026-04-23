namespace WMSSolution.Shared.Excel;

public class InputSku : InputExcelBase
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Specification { get; set; } = string.Empty;
    public string AllowDuplicate { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
