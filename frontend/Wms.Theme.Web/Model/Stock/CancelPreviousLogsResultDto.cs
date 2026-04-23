namespace Wms.Theme.Web.Model.Stock;

public class CancelPreviousLogsResultDto
{
    public string TraceId { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public int CanceledLogs { get; set; }
    public int DeletedConflicts { get; set; }
}