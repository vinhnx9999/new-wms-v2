using WMSSolution.Core.DI;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models.IntegrationWCS;
using WMSSolution.Shared.Enums;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Outbound;
using WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Swap;

namespace WMSSolution.WMS.IServices.IntegrationWCS;

/// <summary>
/// Defines a contract for services that facilitate integration between systems or components.
/// </summary>
/// <remarks>Implementations of this interface typically provide methods for connecting, exchanging data, or
/// coordinating operations across different platforms or modules. The specific integration capabilities are determined
/// by the implementing class.</remarks>
public interface IIntegrationService : IDependency
{
    // TODO Save data Integration
    /// <summary>
    /// Saves data integration for inbound or outbound tasks.   
    /// </summary>
    /// <param name="isInbound">default value to check the type of task</param>
    /// <returns></returns>
    Task<bool> SaveDataIntegration(bool isInbound = true);
    /// <summary>
    /// Asynchronously retrieves a collection of inbound task records.  
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<IEnumerable<InboundTaskResponse>> GetInboundTaskAsync(CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a collection of outbound task records.
    /// </summary>
    /// <returns>An enumerable collection of objects representing outbound tasks.</returns>
    Task<IEnumerable<OutboundTaskResponse>> GetOutboundTaskAsync(CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates the status of an inbound task.
    /// </summary>
    /// <param name="request">The inbound status request containing task details</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <param name="cancellationToken">cancelation token</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateInboundSuccessStatusAsync(InboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates the status of an outbound task.
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="cancellationToken">Cancellation token</param>  
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateOutboundStatusAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Update Pallet Location
    /// </summary>
    /// <param name="palletCode"></param>
    /// <param name="location"></param>
    /// <param name="toLocation"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<bool> UpdatePalletLocationAsync(string palletCode, string location, string toLocation, CurrentUser currentUser);
    /// <summary>
    /// Get Reshuffling
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<SwapPalletDTO>> ReshufflingAsync(CurrentUser currentUser);
    /// <summary>
    /// Update Reshuffling
    /// </summary>
    /// <param name="swapId"></param>
    /// <param name="status"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<bool> UpdateReshufflingAsync(long swapId, IntegrationStatus status, CurrentUser currentUser);

    /// <summary>
    /// Map Locations
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<LocationResponse>> GetMapLocations(Guid? blockId, CurrentUser currentUser, CancellationToken ct);
    /// <summary>
    /// Get Block Locations
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<BlockLocation>> GetBlockLocations(CancellationToken ct);
    /// <summary>
    /// Update Inbound Processing Status
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdateInboundProcessingStatusAsync(InboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);

    #region Inbound Task Creation 

    /// <summary>
    /// Creates InboundEntity records from DTOs.
    /// This method is responsible only for creating and persisting Inbound entities.
    /// </summary>  
    /// <param name="tasks">List of inbound task DTOs</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>List of created InboundEntity objects with assigned IDs</returns>
    Task<List<InboundEntity>> CreateInboundEntitiesAsync(
        List<CreateInboundTaskDTO> tasks,
        CurrentUser currentUser);

    /// <summary>
    /// Creates IntegrationHistory records from InboundEntity list.
    /// This method is responsible only for creating history records for audit/tracking purposes.
    /// </summary>
    /// <param name="inboundEntities">List of inbound entities to create history for</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>True if history records were created successfully</returns>
    Task<bool> CreateIntegrationHistoryAsync(
        List<InboundEntity> inboundEntities,
        CurrentUser currentUser);

    /// <summary>
    /// Get task by condition
    /// </summary>
    /// <param name="request">The inbound task condition request containing filter details</param>
    /// <param name="cancellationToken" >Cancellation token</param>
    /// <param name="currentUser"  >Current user context</param>
    /// <returns>List of inbound tasks matching the condition</returns>
    Task<IEnumerable<InboundTaskResponse>?> GetInboundTaskByStatusAsync(InboundTaskConditionRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="key"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<bool> RejectInboundTaskAsync(InboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);

    #endregion

    #region Outbound Task Operations

    /// <summary>
    /// Get outbound tasks by condition (status and date range)
    /// </summary>
    /// <param name="request">Filter conditions</param>
    /// <returns>Filtered outbound tasks</returns>
    /// <param name="currentUser">  </param>
    /// <param name="cancellationToken"></param>
    Task<IEnumerable<OutboundTaskResponse>?> GetOutboundTaskByStatusAsync(OutboundTaskConditionRequest request, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Reject an outbound task
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>True if rejection was successful, false otherwise</returns>
    Task<bool> RejectOutboundTaskAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Confirms/updates the status of an outbound task as successful
    /// </summary>
    /// <param name="request">The outbound status request containing task details</param>
    /// <param name="currentUser">Current user context</param>
    /// <param name="key">Optional key for additional context or validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateOutboundSuccessStatusAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);

    #endregion

    #region Outbound Task Creation

    /// <summary>
    /// Creates OutboundEntity records from DTOs.
    /// This method is responsible only for creating and persisting Outbound entities.
    /// </summary>
    /// <param name="tasks">List of outbound task DTOs</param>
    /// <param name="currentUser">Current user context</param>
    /// <returns>List of created OutboundEntity objects with assigned IDs</returns>
    Task<List<OutboundEntity>> CreateOutboundEntitiesAsync(
        List<CreateOutboundTaskDTO> tasks,
        CurrentUser currentUser);

    /// <summary>
    /// Outbound Integration History Creation
    /// </summary>
    /// <param name="outboundEntities"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<bool> CreateOutboundIntegrationHistoryAsync(
      List<OutboundEntity> outboundEntities,
      CurrentUser currentUser);

    /// <summary>
    /// Update status for outbound
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdateOutboundProcessingStatusAsync(OutboundStatusRequest request, string? key, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Get List Pallet Location
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<PalletLocationDto>> GetPalletLocationAsync(string blockId, CancellationToken cancellationToken);

    /// <summary>
    /// Sync location data pushed from WCS and upsert conflict records.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="traceID"></param>
    /// <param name="sourceSystem"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool success, string message)> SyncLocationDataAsync(SyncLocationRequest request,
                                                                string traceID,
                                                                string? sourceSystem,
                                                                CurrentUser currentUser,
                                                                CancellationToken cancellationToken);

    #endregion
}
