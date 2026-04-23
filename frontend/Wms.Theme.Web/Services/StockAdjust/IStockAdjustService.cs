using Wms.Theme.Web.Entities.ViewModels;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.StockAdjust;
using Wms.Theme.Web.Model.StockProcess;

namespace Wms.Theme.Web.Services.StockAdjust;

public interface IStockAdjustServices
{
    Task<ResultModel<PageData<StockadjustViewModel>>> GetStockAdjustPageAsync(PageSearchRequest pageSearchRequest);
    Task<ResultModel<string>> CreateStockAdjustAsync(StockAdjustRequest model);
    Task<StockadjustViewModel?> GetDetailAsync(int id);
    Task<ResultModel<PageData<StockSourceSelectionViewModel>>> GetStockSourcesForChangeRequestAsync(PageSearchRequest searchRequest);
    Task<ResultModel<PageData<SkuAdjustmentSelectionViewModel>>> GetSkuForAdjustmentSelectionAsync(PageSearchRequest searchRequest);
    Task<bool> UpdateProcessAsync(StockadjustViewModel request);
    Task<bool> ConfirmAdjustment(int id);
    Task<bool> ConfirmProcess(int id);    
    Task<ResultModel<PageData<StockprocessGetViewModel>>> PageProcessingAsync(PageSearchRequest request);
    Task<string> DeleteStockProcessAsync(int id);
}
