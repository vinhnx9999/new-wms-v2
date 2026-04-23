using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockTaking;

namespace Wms.Theme.Web.Services.StockTaking
{
    public interface IStockTakingService
    {
        Task<ResultModel<PageData<StocktakingViewModel>>> GetStockTakingPageAsync(PageSearchRequest request);
        Task<bool> AddStockTakingAsync(StocktakingBasicViewModel request);
        Task<bool> CountedStockTakingAsync(StocktakingConfirmViewModel request);
        Task<bool> RemoveStockTakingAsync(int id);
        Task<bool> ConfirmStockTakingAsync(int id);
        Task<StocktakingViewModel> GetStockTakingById(int id);
    }
}
