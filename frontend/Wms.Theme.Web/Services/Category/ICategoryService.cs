using Wms.Theme.Web.Model.Category;

namespace Wms.Theme.Web.Services.Category
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllCategory();
    }
}
