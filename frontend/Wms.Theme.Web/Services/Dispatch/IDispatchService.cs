using Wms.Theme.Web.Model.Dispatch;
using Wms.Theme.Web.Model.ShareModel;

namespace Wms.Theme.Web.Services.Dispatch
{
    public interface IDispatchService
    {
        Task<string> GetNextDispatchNoAsync();
        Task<ApiResponse<DispatchListDTO>> GetDispatchList(PageSearchRequest request);
        Task<ApiResponse<DispatchAdvancedDTO>> GetDispatchAdvancedList(PageSearchRequest request);
        Task<bool> AddNewDispatchList(List<DispatchListAddViewModel> request);
        Task<List<DispatchDetailDTO>> GetByDispatchlistNo(string dispatch_no);
        Task<bool> RemoveDispatchList(string dispatch_no);
        Task<List<DispatchlistConfirmDetailViewModel>> GetDispatchByConfirmCheck(string dispatch_no);
        Task<bool> ConfirmDispatchHasOrdered(List<DispatchlistConfirmDetailViewModel> request);
        Task<bool> ConFirmDispatchHasPicked(string dispatch_no);
        Task<bool> ConFirmDispatchHasPackage(List<DispatchListPackageDTO> request);
        Task<bool> CancelOrder(CancelOrderDTO cancelOrderDTO);
        Task<bool> WeighDispatchList(List<DispatchListWeightDTO> request);
        Task<bool> ConfirmDispatchHasDeliveried(List<DispatchListDeliveryDTO> request);
        Task<bool> SignDispatchList(List<DispatchListSignDTO> request);

        /// <summary>
        /// Create draft dispatch order (status = Draft, no stock lock)
        /// POST /api/dispatchlist/draft
        /// </summary>
        Task<DispatchDraftResponse> CreateDraftAsync(DispatchDraftRequest request);

        /// <summary>
        /// Execute draft dispatch (change status to Processing, lock stock)
        /// PUT /api/dispatchlist/execute/{id}
        /// </summary>
        Task<DispatchExecuteResponse> ExecuteDraftAsync(int dispatchId);

        /// <summary>
        /// Create and execute dispatch in one step (skip review)
        /// POST /api/dispatchlist/create-and-execute
        /// </summary>
        Task<DispatchDraftResponse> CreateAndExecuteAsync(DispatchDraftRequest request);
    }
}
