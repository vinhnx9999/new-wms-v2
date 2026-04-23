using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices.Sku;

/// <summary>
/// Interface Unit Of Measure Service
/// </summary>
public interface IUnitOfMeasureService : IBaseService<SkuUomEntity>
{
    /// <summary>
    /// Delete Unit
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteUnitAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get All
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<List<UnitDTO>> GetAllAsync(CurrentUser currentUser);

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelData(List<InputUnitOfMeasure> request, CurrentUser currentUser, CancellationToken cancellationToken);
}

