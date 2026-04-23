/*
 * date：2022-12-20
 * developer：AMo
 */
using WMSSolution.Core.JWT;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of CategoryService
    /// </summary>
    public interface ICategoryService : IBaseService<CategoryEntity>
    {
        #region Api
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<List<CategoryViewModel>> GetAllAsync(CurrentUser currentUser);
        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="id">primary key</param>
        /// <returns></returns>
        Task<CategoryViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(CategoryViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(CategoryViewModel viewModel);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int id);
        #endregion
    }
}

