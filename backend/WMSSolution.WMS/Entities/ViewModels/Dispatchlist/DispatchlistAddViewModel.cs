
namespace WMSSolution.WMS.Entities.ViewModels
{
    /// <summary>
    /// add dispatchlist viewmodel
    /// </summary>
    public class DispatchlistAddViewModel
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public DispatchlistAddViewModel()
        {

        }
        #endregion
        #region Property
        /// <summary>
        /// customer_id
        /// </summary>
        public int customer_id { get; set; } = 0;

        /// <summary>
        /// customer_name
        /// </summary>
        public string customer_name { get; set; } = string.Empty;

        /// <summary>
        /// sku_id
        /// </summary>
        public int sku_id { get; set; } = 0;

        /// <summary>
        /// qty
        /// </summary>
        public int qty { get; set; } = 0;

        /// <summary>
        /// weight
        /// </summary>
        public decimal weight { get; set; } = 0;

        /// <summary>
        /// volume
        /// </summary>
        public decimal volume { get; set; } = 0;
        #endregion
    }
}
