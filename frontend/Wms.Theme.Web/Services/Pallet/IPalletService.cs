using Wms.Theme.Web.Model.Pallet;
using Wms.Theme.Web.Model.ShareModel;

namespace Wms.Theme.Web.Services.Pallet
{
    public interface IPalletService
    {
        Task<ResultModel<List<PalletDto>>?> GetAllAsync();
        Task<CreatePalletResponse?> GetNextPalletCode();
        Task<ResultModel<PalletPageSearchResponse>?> SearchAsync(PageSearchRequest request);
    }
}

