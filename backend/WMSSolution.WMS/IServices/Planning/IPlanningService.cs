using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.Receipt;
using WMSSolution.WMS.Entities.ViewModels.Planning;

namespace WMSSolution.WMS.IServices.Planning;

/// <summary>
/// Planning Service
/// </summary>
public interface IPlanningService : IBaseService<OutBoundReceiptEntity>
{
    /// <summary>
    /// Get Picking List
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<PickingDTO>> GetPickingList(CurrentUser currentUser);
    /// <summary>
    /// Get Planning Packing List
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<PickingDTO>> GetPlanningPackingList(CurrentUser currentUser);

    /// <summary>
    /// Save Picking List
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool Success, string? Message)> SavePickingList(IEnumerable<PickingDTO> requests, 
        CurrentUser currentUser, CancellationToken cancellationToken);
}
