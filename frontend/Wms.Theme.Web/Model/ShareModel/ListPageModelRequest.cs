namespace Wms.Theme.Web.Model.ShareModel
{
    public class ListPageModelRequest
    {
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

