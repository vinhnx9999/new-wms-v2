using Wms.Theme.Web.Model.Planning;

namespace Wms.Theme.Web.Services.Planning;

public interface IPlanningService
{
    Task<IEnumerable<PickingDTO>> GetPackingList();
    Task<IEnumerable<PickingDTO>> GetPickingList();
    Task<(bool Success, string? Message)> SavePickingList(IEnumerable<PickingDTO> requests);
}
