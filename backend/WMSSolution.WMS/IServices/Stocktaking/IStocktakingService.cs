/*
 * date：2022-12-30
 * developer：AMo
 */
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
 using WMSSolution.WMS.Entities.Models;
 using WMSSolution.WMS.Entities.ViewModels;
 
 namespace WMSSolution.WMS.IServices
 {
     /// <summary>
     /// Interface of StocktakingService
     /// </summary>
     public interface IStocktakingService : IBaseService<StocktakingEntity>
     {
         #region Api
         /// <summary>
         /// page search
         /// </summary>
         /// <param name="pageSearch">args</param>
         /// <param name="currentUser">current user</param>
         /// <returns></returns>
         Task<(List<StocktakingViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);
          
         /// <summary>
         /// Get a record by id
         /// </summary>
         /// <param name="id">primary key</param>
         /// <returns></returns>
         Task<StocktakingViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(StocktakingBasicViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// update  counted_qty
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> PutAsync(StocktakingConfirmViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// confrim a record and change stock and add to stockadjust
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmAsync(int id, CurrentUser currentUser);
 
         /// <summary>
         /// delete a record
         /// </summary>
         /// <param name="id">id</param>
         /// <returns></returns>
         Task<(bool flag, string msg)> DeleteAsync(int id);
         #endregion
     }
 }
 
