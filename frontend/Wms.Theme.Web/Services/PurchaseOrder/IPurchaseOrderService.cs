using Wms.Theme.Web.Model.PurchaseOrder;
using Wms.Theme.Web.Model.ShareModel;

namespace Wms.Theme.Web.Services.PurchaseOrder
{
    public interface IPurchaseOrderService
    {
        Task<ResultModel<PageData<PageSearchPOResponse>>> GetPageAsync(PageSearchRequest pageSearchRequest);
        Task<bool> CreateAsync(CreatePoRequest request);
        Task<ApiResult<bool>> UpdateAsync(CreateNewOrderRequest request);
        Task<ApiResult<string>> DeleteAsync(int id);
        Task<ApiResult<List<CreateNewOrderRequest>>> GetOpenPosAsync();
        Task<ApiResult<string>> CloseAsync(int id);

        /// <summary>
        /// Genarate a new PO number
        /// </summary>
        /// <returns></returns>
        Task<string> GeneratePoNo();
        Task<ApiResult<PoDetailDto>> GetDetailAsync(int id);
    }
}
