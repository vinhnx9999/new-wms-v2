using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;

namespace Wms.Theme.Web.Services.AsnMaster;

public interface IAsnMasterService
{
    Task<AsnMasterApiResponse?> GetAsnMasterListAsync(ListPageModelRequest request);
    Task<AsnMasterCreateResponse?> CreateAsnMasterAsync(AsnMasterCustomDetailedDTO request);
    Task<AsnMasterCreateResponse?> CreateDraftAsync(AsnMasterCustomDetailedDTO request);
    Task<AsnMasterCreateResponse?> SubmitAsync(AsnMasterCustomDetailedDTO request);
    Task<string> GetNextAsnNo();
    Task<AsnMasterCustomDetailedDTO?> GetAnsMasterDetailed(int id);
    Task<bool> UpdateAnsMaster(AsnMasterCustomDetailedDTO request);
    Task<ResultModel<string>?> CompleteAsnMasterAsync(int id);
    Task<ResultModel<string>?> DeleteAsnMasterAsync(int id);

    Task<ResultModel<string>?> RetryInboundItemAsync(RetryInboundItemRequest request);
}
