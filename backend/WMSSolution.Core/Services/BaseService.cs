using WMSSolution.Core.Models;

namespace WMSSolution.Core.Services
{
    /// <summary>
    /// Base Service
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : BaseModel
    {

    }
}
