using Wms.Theme.Web.Model.InboundReceipt;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Stock;

public interface IBeginMerchandiseService
{
    Task<(int? data, string? message)> DeleteBeginning(int id);
    Task<IEnumerable<BeginMerchandiseDto>> GetBeginMerchandises();
    Task<(int? data, string? message)> ImportExcelData(List<BeginMerchandiseExcel> inputOrders);
    Task<(int? data, string? message)> SaveBeginning();
}
