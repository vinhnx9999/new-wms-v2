namespace Wms.Theme.Web.Model.StockProcess
{
    public class StockprocessViewModel
    {
        #region Property

        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; } = 0;

        /// <summary>
        /// job_code
        /// </summary>
        public string job_code { get; set; } = string.Empty;

        /// <summary>
        /// job_type
        /// </summary>
        public bool job_type { get; set; } = true;

        /// <summary>
        /// process_status
        /// </summary>
        public bool process_status { get; set; } = true;

        /// <summary>
        /// processor
        /// </summary>
        public string processor { get; set; } = string.Empty;

        /// <summary>
        /// process_time
        /// </summary>

        public DateTime process_time { get; set; }

        /// <summary>
        /// creator
        /// </summary>
        public string creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// last_update_time
        /// </summary>
        public DateTime last_update_time { get; set; }

        /// <summary>
        /// tenant_id
        /// </summary>
        public long tenant_id { get; set; } = 0;


        #endregion

        #region detail table

        /// <summary>
        /// detail table
        /// </summary>
        public List<StockprocessdetailViewModel> detailList { get; set; } = new List<StockprocessdetailViewModel>(2);
        
        #endregion

    }
}

