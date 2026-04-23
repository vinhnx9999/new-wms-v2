using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockMove;

namespace Wms.Theme.Web.Services.StockMove
{
    public interface IStockMoveService
    {
        Task<ResultModel<PageData<StockmoveViewModel>>> GetStockPageAsync(PageSearchRequest request);
        Task<StockmoveViewModel> GetStockMoveByIdAsync(int id);
        Task<bool> CreateStockMoveAsync(StockmoveViewModel stockMove);
        Task<bool> ConfirmStockMoveAsync(int id);
        Task<bool> RemoveStockMoveAsync(int id);
        Task<StockMoveDashboardStats> GetDashboardStatsAsync();
    }
}
