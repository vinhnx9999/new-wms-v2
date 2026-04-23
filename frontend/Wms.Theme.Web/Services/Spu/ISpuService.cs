using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.SPU;
using Wms.Theme.Web.Model.Stock;

namespace Wms.Theme.Web.Services.Spu
{
    public interface ISpuService
    {
        Task<ApiResponse<SpuDto>?> getSpuListAsync(ListPageModelRequest model);
        Task<List<SkuUomDTO>> GetSkuUomListAsync();
    }
}
