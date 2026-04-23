using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockProcess;

namespace Wms.Theme.Web.Services.StockProcess
{
    public interface IStockProcessService
    {
        Task<ResultModel<PageData<StockprocessGetViewModel>>> PageAsync(PageSearchRequest request);
        Task<string> AddAsync(StockprocessViewModel request);
        Task<string> DeleteStockProcessAsync(int id);
        Task<StockprocessWithDetailViewModel> GetDetailAsync(int id);
        /// <summary>
        /// lock the ticket for adjust
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ConfirmProcess(int id);
        /// <summary>
        /// manager Appove for handle it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ConfirmAdjustment(int id);
        Task<bool> UpdateProcessAsync(StockprocessViewModel request);
        Task<StockProcessDashboardStatsViewModel> GetDashboardStatsAsync();
    }
}
