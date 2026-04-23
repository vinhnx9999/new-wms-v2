using WMSSolution.Core.DynamicSearch;

namespace WMSSolution.Core.Models
{
    /// <summary>
    /// PageSearch
    /// </summary>
    public class PageSearch
    {
        /// <summary>
        /// Get all data when pageIndex is 0, otherwise get data by page
        /// </summary>
        public bool GetAllSearch => pageIndex == 0;

        /// <summary>
        /// current page number
        /// </summary> 
        public int pageIndex { get; set; } = 1;

        /// <summary>
        /// rows per page
        /// </summary>
        public int pageSize { get; set; } = 20;

        /// <summary>
        /// Custom Classification
        /// </summary>
        public string sqlTitle { get; set; } = "";

        /// <summary>
        /// search condition
        /// </summary>
        public List<SearchObject> searchObjects { get; set; } = new List<SearchObject>();
    }
}
