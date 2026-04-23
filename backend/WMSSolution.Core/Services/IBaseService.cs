using WMSSolution.Core.DI;
using WMSSolution.Core.Models;
namespace WMSSolution.Core.Services
{
    /// <summary>
    /// Base Service interface for apply all service DI
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IBaseService<TEntity> : IDependency where TEntity : BaseModel
    {

    }
}
