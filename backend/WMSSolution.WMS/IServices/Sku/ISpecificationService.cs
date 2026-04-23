using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices.Sku;
/// <summary>
/// Specification
/// </summary>
public interface ISpecificationService : IBaseService<SpecificationEntity>
{   
    /// <summary>
    /// Delete Specification
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool flag, string? msg)> DeleteSpecificationAsync(int id, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelData(List<InputSpecification> request, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get All
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<List<SpecificationDTO>> GetAllAsync(CurrentUser currentUser);
}
