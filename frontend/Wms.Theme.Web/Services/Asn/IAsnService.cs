using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.Delivery;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sorted;
using Wms.Theme.Web.Model.Unload;

namespace Wms.Theme.Web.Services.Asn
{
    public interface IAsnService
    {
        Task<AsnApiResponse> GetAsnListAsync(PageSearchRequest request);
        Task<AsnDetailApiResponse> GetAsnDetailsAsync(int asnId);
        Task<AsnUpdateResponse> UpdateAsnAsync(AsnUpdateRequest request);
        Task<AsnCreateResponse> CreateAsnAsync(AsnUpdateRequest request);
        Task<AsnDeleteResponse> DeleteAsnAsync(int asnId);
        Task<(bool success, string message)> ConfirmForDeliveryAsnAsync(List<ConfirmModel> request);
        Task<bool> RejectForDeliveryAsnAsync(List<int> request);
        Task<(bool success, string message)> ConfirmForUnLoadAsnAsync(List<UnloadConfirmRequest> request);
        Task<(bool success, string message)> RejectUnloadAsync(List<int> request);
        Task<(bool success, string message)> AddedAsnToAnsSortedAsync(List<AddAnsToAnsSortedRequest> request);
        Task<bool> UpdateAsnSortedAsync(List<UpdateAnsSortedRequest> requests);
        Task<List<AnsSortedResponse>> GetAnsSortedByAnsIDAsync(int id);
        Task<(bool success, string message)> ConfirmForSortedAsnAsync(List<int> ids);
        Task<(bool success, string message)> RejectForSortedAsnAsync(List<int> ids);
        Task<List<GetAsnPutawayResponse>> GetAsnPendingPutawayAsync(int id);
        Task<(bool success, int flag, string message)> UpdatePutaway(List<UpdatePutawayRequest> requests);
        Task<List<GetAsnQrCodeRequest>> ShowQrCode(List<int> id);
        Task<Dictionary<int, int>> GetTotalRecord();

        /// <summary>
        /// Confirm Robot has Success To putaway
        /// </summary>
        /// <param name="ids">List of ASN IDs to confirm</param>
        /// <returns>Tuple of success boolean and message string</returns>
        Task<(bool success, string message)> ConfirmRobotSuccessAsync(List<int> ids);
    }
}
