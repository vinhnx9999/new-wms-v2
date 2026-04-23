using Wms.Theme.Web.Model.ASN;

namespace Wms.Theme.Web.Services.GoodsOwner
{
    public interface IGoodOwnerService
    {
        Task<List<GoodOwnerDTO>> GetAllGoodOwner();
    }
}
