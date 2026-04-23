using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of RolemenuService
    /// </summary>
    public interface IRolemenuService : IBaseService<RolemenuEntity>
    {
        #region Api
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<List<RolemenuListViewModel>> GetAllAsync(CurrentUser currentUser);

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="userrole_id">userrole id</param>
        /// <returns></returns>
        Task<RolemenuBothViewModel> GetAsync(int userrole_id);

        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(RolemenuBothViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// Get all menus
        /// </summary>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<List<MenuViewModel>> GetAllMenusAsync(CurrentUser currentUser);

        /// <summary>
        /// Get menu's authority by user role id
        /// </summary>
        /// <param name="userrole_id">user role id</param>
        /// <returns></returns>
        Task<List<MenuViewModel>> GetMenusByRoleId(int userrole_id);

        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(RolemenuBothViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="userrole_id">userrole id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int userrole_id);
        #endregion
    }
}
