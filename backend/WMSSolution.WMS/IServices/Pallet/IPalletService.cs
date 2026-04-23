
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.Pallet;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Pallet;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of PalletService
    /// </summary>
    public interface IPalletService : IBaseService<PalletEntity>
    {
        #region Api
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="currentUser">current user</param>
        /// <returns>list of pallets</returns>
        Task<List<PalletViewModel>> GetAllAsync(CurrentUser currentUser);

        /// <summary>
        /// Auto generate pallet code
        /// </summary>
        /// <param name="currentUser">current user</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        Task<CreatePalletResponse?> GenaratePalletCodeAsync(CurrentUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Page search pallets
        /// </summary>
        /// <param name="pageSearch">search parameters</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <param name="currentUser">current user</param>
        /// <returns>list of pallets with total count</returns>
        Task<(List<PalletPageSearchDTO> data, int total)> PageSearchPallet(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
        #endregion
    }
}

